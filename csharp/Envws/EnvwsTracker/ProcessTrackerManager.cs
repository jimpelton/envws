using System;
using System.Threading.Tasks;

namespace EnvwsTracker
{
    using log4net;

    using EnvwsLib.Util;
    using EnvwsLib.Tracker;
    using EnvwsLib.DataContracts;
    using TrackProcess;

    public class ProcessTrackerManager
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(ProcessTrackerManager));

        /// <summary>
        /// Gets a reference to this trackers current status (RUNNING, IDLE, etc).
        /// </summary>
        public TrackerStatus Status
        {
            get
            {
                lock (statusMutex)
                {
                    return  status;
                }
            }

            private set
            {
                lock (statusMutex)
                {
                     status = value;
                }
            }
        }
        private TrackerStatus status = TrackerStatus.UNKNOWN;
        private readonly object statusMutex = new object();

        
        private readonly WFQueue<JobData> jobqueue = new WFQueue<JobData>();

        private Task watchTask;

        private bool KeepWorking
        {
            get
            {
                lock ( keepWorkingMutex)
                {
                    return  keepWorking;
                }
            }

            set
            {
                lock ( keepWorkingMutex)
                {
                     keepWorking = value;
                }
            }
        }
        private bool keepWorking = false;
        private readonly object keepWorkingMutex = new object();


        public event EventHandler<StatusChangedEventArgs> StatusChanged;

        public event EventHandler<JobCompletedEventArgs> JobCompleted;

        public ProcessTrackerManager() { }

        /// <summary>
        /// Adds a new job to the job queue.
        /// </summary>
        /// <param name="job">
        /// The job to add.
        /// </param>
        public void AddNewJob(JobData job)
        {
             jobqueue.EnqueueWaiting(job);
             Status = TrackerStatus.RUNNING;
        }

        /// <summary>
        /// Returns the size of the waiting job queue.
        /// </summary>
        /// <returns>
        /// An int that is the number of jobs waiting in the job queue.
        /// </returns>
        public int NumWaiting()
        {
            return  jobqueue.SizeWaiting();
        }

        /// <summary>
        /// Start the watch task and job processing loop.
        /// </summary>
        public void Start()
        {
            if (! KeepWorking)
            {
                 KeepWorking = true;
                 watchTask = new Task( Watch);
                 watchTask.Start();
            }
        }

        /// <summary>
        /// Watch for a new job, then execute it.
        /// </summary>
        private void Watch()
        {
            Status = TrackerStatus.IDLE;
            while ( KeepWorking)
            {
                logger.Info("Waiting for new job.");
                JobData job =  jobqueue.DequeueWaiting();
                Status = TrackerStatus.RUNNING;

                logger.Info("New job dequeued: " + job.Guid);
                logger.Info("Starting job." + job.Guid);

                job.StartTime = Utils.CurrentUTCMillies();
                
                //TODO: use a factory to create JobRunners.
                int exitCode = new EnvisionJobRunner(job).ExecuteJob();
                job.EnvisionExitCode = exitCode;
                
                logger.Info("Envision exit code: " + exitCode);
                job.FinishTime = Utils.CurrentUTCMillies();

                jobqueue.EnqueueDone(job);
                Status = TrackerStatus.IDLE;
                logger.Info(string.Format("Job finished {0} ({1})", job.FriendlyName, job.Guid));

                //send job completed event
                JobCompletedEventArgs args = new JobCompletedEventArgs { Job = job };
                OnJobCompleted(args);
            }
             logger.Info("Watch loop exiting.");
        }

        protected virtual void OnJobCompleted(JobCompletedEventArgs e)
        {
            EventHandler<JobCompletedEventArgs> handler = JobCompleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }


    }
}
