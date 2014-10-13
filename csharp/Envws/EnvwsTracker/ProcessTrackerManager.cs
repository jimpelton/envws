using System;
using System.Threading.Tasks;


namespace EnvwsTracker
{
    using log4net;

    using EnvwsLib.Events;
    using EnvwsLib.Util;
    using EnvwsLib.Tracker;
    using EnvwsLib.DataContracts;

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
                try
                {
                    watchTask = new Task(Watch);
                    watchTask.Start();
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                    Console.Error.WriteLine(e.StackTrace);
                }
            }
        }

        /// <summary>
        /// Watch for a new job, then execute it.
        /// </summary>
        private void Watch()
        {
            Status = TrackerStatus.IDLE;
            while (KeepWorking)
            {
                logger.Info("Waiting for new job.");
                
                JobData job =  jobqueue.DequeueWaiting();
                Status = TrackerStatus.RUNNING;

                logger.Info(string.Format("Starting new job {0} ({1}).", job.FriendlyName, job.Guid));

                job.StartTime = Utils.CurrentUTCMillies();
                
                int exitCode = RunSynchronousTask(job);

                job.FinishTime = Utils.CurrentUTCMillies();
                job.EnvisionExitCode = exitCode;

                if (exitCode != 0)
                    job.Status = JobStatus.FAILED;
                else
                    job.Status = JobStatus.COMPLETE;

                logger.Info("Envision exit code: " + exitCode);

                jobqueue.EnqueueDone(job);
                Status = TrackerStatus.IDLE;
                
                logger.Info(string.Format("Job finished {0} ({1})", job.FriendlyName, job.Guid));

                //send job completed event
                JobCompletedEventArgs args = new JobCompletedEventArgs { Job = job };
                OnJobCompleted(args);
            }
            
            logger.Info("Watch loop exiting.");
        }

        /// <summary>
        /// Create a JobRunner and execute it in a separate task. This method blocks
        /// until the task is finished.
        /// 
        /// Catches exceptions swallowed by the task, unrolls and logs 'em.
        /// When the JobRunner is done, this method returns the exit code returned
        /// by the JobRunner.
        /// </summary>
        /// <param name="job">The job to hand off to a job runner.</param>
        /// <returns>Exit code from the runner, -1 if the jobrunner failed.</returns>
        private int RunSynchronousTask(JobData job)
        {
            int exitCode = -1;
            try
            {
                //TODO: use a factory to create JobRunners.
                Task<bool> exeTask = new Task<bool>(
                    () => new EnvisionJobRunner(job).ExecuteJob(ref exitCode));
                
                exeTask.Start();
                exeTask.Wait();   // <-- the synchronous part.
            }
            catch (AggregateException ae)
            {
                foreach (var exception in ae.Flatten().InnerExceptions)
                {
                    logger.Error(exception);
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }

            return exitCode;
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
