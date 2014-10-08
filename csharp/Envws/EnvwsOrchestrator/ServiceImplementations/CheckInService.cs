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

        public static TrackerQueue Q { get; set; }

        public bool CheckIn(TrackerData td)
        {
            td.LastCheckinTime = Utils.CurrentUTCMillies();

            logger.Debug("Tracker just checked in: " + td);
            
            Q.UpdateTracker(td);
            return true;
        }

        public JobData RequestJob()
        {
            JobData j;
            Q.GetJob(out j);
            return j;
        }

        public void ReturnFinishedJob(JobData j)
        {
            Q.PushFinishedJob(j);
        }

        //private string GetIP()
        //{
        //    OperationContext context = OperationContext.Current;
        //    MessageProperties prop = context.IncomingMessageProperties;
        //    RemoteEndpointMessageProperty endpoint =
        //       prop[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
        //    string ip = endpoint.Address;
            
        //    return ip;
        //}
    }
}