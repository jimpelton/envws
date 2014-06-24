

namespace EnvwsOrchestrator
{
    using log4net;
    using EnvwsLib.ServiceContracts;
    using EnvwsLib.DataContracts;
    using EnvwsLib.Tracker;

    public class OrchestratorService : IOrchestratorService
    {
        private static ILog
            logger = LogManager.GetLogger(typeof(OrchestratorService));

        public static TrackerQueue TrackerQueue { get; set; }

        public bool QueueJob(JobData job)
        {
            logger.Info("Queueing new job: " + job);
            return TrackerQueue.PushJob(job);
        }

        public bool RemoveJob(string jobGuid)
        {
            return false;
        }

        public TrackerData[] TrackerStatus()
        {
            logger.Info("Updating tracker status per client request.");
            TrackerData[] trackerData;
            TrackerQueue.GetTrackersArray(out trackerData);
            return trackerData;
        }

        public bool Ping()
        {
            return true;
        }

        public int NumWaitingJobs()
        {
            logger.Info("NumWaitingJobs called");
            return TrackerQueue.NumWaitingJobs();
        }

        public int NumIdleTrackers()
        {
            logger.Info("NumIdleTrackers called");
            return TrackerQueue.IdleTrackers();
        }

        public int NumRunningTrackers()
        {
            logger.Info("NumRunningTrackers called");
            return TrackerQueue.RunningTrackers();
        }
    } // class OrchestratorService
}
