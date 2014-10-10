
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace EnvwsOrchestrator
{
    using log4net;

	using EnvwsLib.Util;
    using EnvwsLib.DataContracts;
    using System.Collections.Generic;

    public class TrackerQueue
    {
        private readonly ILog logger;

        /// <summary>
        /// All trackers in this orchestrators known universe.
        /// </summary>
        private readonly ConcurrentDictionary<Guid, TrackerData>
            allTrackers = new ConcurrentDictionary<Guid, TrackerData>();

        /// <summary>
        /// Jobs just delivered, not sent to trackers yet.
        /// </summary>
        private readonly ConcurrentQueue<JobData> 
            waitingJobs = new ConcurrentQueue<JobData>();

        /// <summary>
        /// Jobs currently handed out to trackers.
        /// </summary>
        private readonly ConcurrentDictionary<Guid, JobData>
            runningJobs = new ConcurrentDictionary<Guid, JobData>();
        
        /// <summary>
        /// Jobs that have been successfuly completed.
        /// </summary>
        private readonly ConcurrentDictionary<Guid, JobData>
            finishedJobs = new ConcurrentDictionary<Guid, JobData>();

        /// <summary>
        /// Maximum interval in milliseconds between tracker checkins.
        /// </summary>
        private readonly long lateCheckinLimit;

        private int idleTrackersCount;
        private int runningTrackersCount;
        

        /// <summary>
        /// Calls OnScrubTimer and scrubs the tracker queues.
        /// </summary>
        private Timer scrubTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerQueue"/> class. 
        /// The queue will scrub the list of known trackers every maxPingSpanMillis/2
        /// milliseconds. If a Tracker has not checked in for maxPingSpanMillis milliseconds
        /// then its status will be set TrackerStatus.NO_RESPONSE.
        /// </summary>
        /// <param name="maxPingSpanMillis">
        /// The time span for NO_RESPONSE
        /// </param>
        public TrackerQueue(int maxPingSpanMillis)
        {
            logger = LogManager.GetLogger(typeof(TrackerQueue));
            lateCheckinLimit = maxPingSpanMillis < 1000 ? 1000 : maxPingSpanMillis;
            scrubTimer = new Timer(this.OnScrubTimer, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Start the loop that scrubs the trackers list for late trackers.
        /// </summary>
        public void StartScrubLoop()
        {
            scrubTimer.Change(1000, Timeout.Infinite);
        }

        /// <summary>
        /// Put JobData j into the waiting queue.
        /// </summary>
        /// <param name="j">
        /// The jobdata that should be added.
        /// </param>
        /// <returns>
        /// True, always true!
        /// </returns>
        public bool PushJob(JobData j)
        {
            waitingJobs.Enqueue(j);
            logger.Info("Put job " + j.Guid + " in the queue");

            return true; 
        }
       
        /// <summary>
        /// Initializes j with the job next in the waiting jobs queue. If no jobs available j is initialized
        /// to null and false is returned.
        /// </summary>
        /// <param name="j">
        /// A jobdata that will be initialized with then next job in the queue.
        /// </param>
        /// <returns>
        /// True if successfully initialized j with a job, false otherwise, or false if there are no waiting jobs.
        /// </returns>
        public bool GetJob(out JobData j)
        {
            j = null;
            if (waitingJobs.Count <= 0)
            {
                return false;
            }

            return waitingJobs.TryDequeue(out j) && 
                runningJobs.TryAdd(Guid.Parse(j.Guid), j);
        }

        /// <summary>
        /// Put a finished job into the finished jobs list.
        /// </summary>
        /// <param name="j">
        /// The JobData that has been finished
        /// </param>
        public void PushFinishedJob(JobData j)
        {
            JobData n;
            finishedJobs.TryAdd(Guid.Parse(j.Guid), j);
            runningJobs.TryRemove(Guid.Parse(j.Guid), out n);
        }

        public JobData[] GetAllJobs()
        {
            List<JobData> jobs = new List<JobData>();
            jobs.AddRange(finishedJobs.Values);
            jobs.AddRange(runningJobs.Values);
            jobs.AddRange(waitingJobs);

            return jobs.ToArray();     
        }

        /// <summary>
        /// Returns the number of jobs waiting to be sent off to a tracker.
        /// </summary>
        /// <returns>
        /// An int that is the number of jobs waiting in the queue.
        /// </returns>
        public int NumWaitingJobs()
        {
            return waitingJobs.Count;
        }

        /// <summary>
        /// Counts number of Idle trackers.
        /// </summary>
        /// <returns>
        /// Number of trackers idle.
        /// </returns>
        public int IdleTrackers()
        {
            //return allTrackers.Count(kvp => kvp.Value.Status == TrackerStatus.IDLE);
            return idleTrackersCount;
        }

        /// <summary>
        /// Counts the number of running trackers.
        /// </summary>
        /// <returns>
        /// The number of running trackers.
        /// </returns>
        public int RunningTrackers()
        {
            //return allTrackers.Values.Count(kvp => kvp.Status == TrackerStatus.RUNNING);
            return runningTrackersCount;
        }

        /// <summary>
        /// called after a tracker checks in and delivers its new TrackerData.
        /// </summary>
        /// <param name="td">
        /// The updated TrackerData for some tracker.
        /// </param>
        public void UpdateTracker(TrackerData td)
        {
            lock (allTrackers)
            {
                allTrackers.AddOrUpdate(Guid.Parse(td.Guid), td, (g, t) => td);
            }
        }

        /// <summary>
        /// Enqueue a new Tracker into the all trackers queue.
        /// </summary>
        /// <param name="client">
        /// The TrackerData sent from the tracker
        /// </param>
        /// <returns>
        /// true if added successfully, false otherwise
        /// </returns>
        public bool EnqueueNewTracker(TrackerData client)
        {
            lock (allTrackers)
            {
                return allTrackers.TryAdd(Guid.Parse(client.Guid), client);
            }
        }

        /// <summary>
        /// Return a list of trackers. The <code>trackers</code> parameter will
        /// be allocated and populated with ref's to the TrackerDatas in this queue.
        /// </summary>
        /// <param name="trackers">
        /// A handle to an uninitialized array of TrackerData.
        /// </param>
        public void GetTrackersArray(out TrackerData[] trackers)
        {
            lock (allTrackers)
            {
                trackers = new TrackerData[allTrackers.Count];
                allTrackers.Values.CopyTo(trackers, 0);
            }
        }
        
        private void OnScrubTimer(object state)
        {
            int lateness = FindLateTrackersAndSetNoResponse();
            logger.Debug(lateness + " trackers have not checked in.");
            scrubTimer.Change(1000, Timeout.Infinite);
        }

        private int FindLateTrackersAndSetNoResponse()
        {
            int lateTrackersCount = 0;
            int idle = 0;
            int running = 0;
            lock (allTrackers)
            {
                foreach (TrackerData td in allTrackers.Values)
                {
                    if (IsLate(td))
                    {
                        td.Status = TrackerStatus.NO_RESPONSE;
                        lateTrackersCount++;
                        logger.Debug("Tracker " + td.Guid + " has been marked as late.");
                    }
                    else
                    {
                        switch (td.Status)
                        {
                            case TrackerStatus.RUNNING:
                                running++;
                                break;
                            case TrackerStatus.IDLE:
                                idle++;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            idleTrackersCount = idle;
            runningTrackersCount = running;
            
            return lateTrackersCount;
        }

        private bool IsLate(TrackerData td)
        {
            bool late = TimeSinceLastCheckin(td) > this.lateCheckinLimit;
            return late;
        }

        private long TimeSinceLastCheckin(TrackerData td)
        {
            return Utils.CurrentUTCMillies() - td.LastCheckinTime;
        }
    }
}
