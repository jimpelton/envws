
using System;
using System.Collections.Concurrent;
using System.Linq;
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
        private readonly List<TrackerData>
            allTrackers = new List<TrackerData>();

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
            runningJobs.TryRemove(Guid.Parse(j.Guid), out n);
            finishedJobs.TryAdd(Guid.Parse(j.Guid), j);    

            logger.InfoFormat("Put finished job {0} ({1}) with status {2} into finished jobs queue.", j.FriendlyName, j.Guid, j.Status.ToString());
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
            return runningTrackersCount;
        }

        /// <summary>
        /// Update the tracker specified by <code>td</code> with the 
        /// info from <code>td</code>.
        /// </summary>
        /// <param name="td">
        /// The updated TrackerData for some tracker.
        /// </param>
        public void UpdateTracker(TrackerData td)
        {
            td.LastCheckinTime = Utils.CurrentUTCMillies();
            lock (allTrackers)
            {
                int i = allTrackers.FindIndex(t => t.Equals(td));
                if (i < 0)
                    allTrackers.Add(td);
                else
                    allTrackers[i] = td;
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
        public void EnqueueNewTracker(TrackerData client)
        {
            lock (allTrackers)
            {
                if (! allTrackers.Contains(client))
                {
                    allTrackers.Add(client);
                }
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
                allTrackers.CopyTo(trackers, 0);
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

            long ctime = Utils.CurrentUTCMillies();
            
            //TODO: oh man, this could be better! ;)
            IEnumerable<TrackerData> removedOnes, lateOnes;

            lock (allTrackers)
            {
                // check for trackers that are already late and remove if really, really late.
                removedOnes = allTrackers.Where(td => TrackerShouldBeRemoved(td, ctime));
                foreach (TrackerData td in removedOnes)
                {
                    allTrackers.Remove(td);
                    if (!RequeueJobIfNeeded(td))
                    {
                        PushFinishedJob(td.CurrentJob);
                    }

                    logger.InfoFormat("Tracker {0} was removed from list of trackers ({1} milliseconds late).",
                                  td.HostName, TimeSinceLastCheckin(td, ctime));
                }

                // find late trackers and mark them as late.
                lateOnes = allTrackers.Where(td => IsLate(td, ctime));
                foreach (TrackerData td in lateOnes)
                {
                    td.Status = TrackerStatus.NO_RESPONSE;
                    lateTrackersCount++;
                    logger.Info("Tracker " + td.HostName + " has been marked as late.");
                }

                // Count individual totals for idle and running trackers.
                foreach (TrackerData td in allTrackers)
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
            } // lock (allTrackers)

            idleTrackersCount = idle;
            runningTrackersCount = running;
            
            return lateTrackersCount;
        }

        private bool RequeueJobIfNeeded(TrackerData td)
        {
            bool wasRemoved = false;
            JobData jd = td.CurrentJob;

            if (!jd.Equals(JobData.EmptyJob) && jd.Status == JobStatus.RUNNNG)
            {
                JobData removedJob = null;
                wasRemoved = runningJobs.TryRemove(Guid.Parse(jd.Guid), out removedJob);
                if (wasRemoved)
                {
                    removedJob.Status = JobStatus.QUEUED;
                    PushJob(removedJob);

                    logger.InfoFormat("Job {0} ({1}) was requeued because its tracker went offline.",
                        removedJob.FriendlyName, removedJob.Guid);
                }
            }

            return wasRemoved;
        }

        private bool TrackerShouldBeRemoved(TrackerData td, long currTime)
        {
            return TimeSinceLastCheckin(td, currTime) > 3*lateCheckinLimit &&
                   td.Status == TrackerStatus.NO_RESPONSE;
        }

        private bool IsLate(TrackerData td, long currTime)
        {
            return TimeSinceLastCheckin(td, currTime) > lateCheckinLimit;
        }

        private long TimeSinceLastCheckin(TrackerData td, long curTime)
        {
            return curTime - td.LastCheckinTime;
        }
    }
}
