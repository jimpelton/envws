using System;


namespace EnvwsLib.Tracker
{
    using DataContracts;
    using Events;

    public abstract class JobRunner
    {
        /// <summary>
        /// Gets a reference to the JobData of the currently running job.
        /// </summary>
        public JobData CurrentJob { get; private set; }

        public event EventHandler<StatusChangedEventArgs> StatusChanged;

        public JobRunner(JobData job)
        {
            CurrentJob = job;
        }

        public abstract bool ExecuteJob(ref int exitCode);

        protected virtual void OnStatusChanged(StatusChangedEventArgs e)
        {
            EventHandler<StatusChangedEventArgs> handler = StatusChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

//        /// <summary>
//        /// Called when a job is completed, and calls the user supplied EventHandler function.
//        /// </summary>
//        /// <param name="e">
//        /// Ya know.
//        /// </param>
//        protected virtual void OnJobCompleted(JobCompletedEventArgs e)
//        {
//            EventHandler<JobCompletedEventArgs> handler = JobCompleted;
//            if (handler != null)
//            {
//                handler(this, e);
//            }
//        }
    }
}
