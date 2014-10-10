using System.ServiceModel;
using System.ServiceModel.Channels;
using log4net;

namespace EnvwsOrchestrator
{
    using EnvwsLib;
    using EnvwsLib.DataContracts;
    using EnvwsLib.ServiceContracts;
    using EnvwsLib.Util;


    public class CheckInService : ICheckInService
    {
        private static ILog
            logger = LogManager.GetLogger(typeof(OrchestratorService));

        public static TrackerQueue TrackerQueue { get; set; }

        public bool CheckIn(TrackerData td)
        {
            logger.Debug("Tracker just checked in: " + td);
            TrackerQueue.UpdateTracker(td);

            return true;
        }

        public JobData RequestJob()
        {
            JobData j;
            TrackerQueue.GetJob(out j);
            
            return j;
        }

        public void ReturnFinishedJob(JobData j)
        {
            TrackerQueue.PushFinishedJob(j);
        }
    }
}