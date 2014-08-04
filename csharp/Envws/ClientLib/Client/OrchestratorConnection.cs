using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Threading;

namespace ClientLib.Client
{
    using EnvwsLib.ServiceProxies;
    using EnvwsLib.DataContracts;

    using log4net;

    public class OrchestratorConnection
    {
        private static ILog 
            logger = LogManager.GetLogger(typeof(OrchestratorConnection));

        private ConcurrentDictionary<Guid, TrackerData> trackers; 
        
        private OrchestratorServiceClientProxy service;
        
        private Timer pingTimer;

        private bool isInit = false;

        public event EventHandler<TrackersPingedEventArgs> TrackersPinged;

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ConfigurationErrorsException">
        /// If a configuration file could not be found.
        /// </exception>
        /// <exception cref="EndpointNotFoundException">
        /// If the endpoint specified in the App.config file was not found.
        /// </exception>
        public OrchestratorConnection()
        {
            trackers = new ConcurrentDictionary<Guid, TrackerData>();

            string exeCmd = Path.Combine(Environment.CurrentDirectory, Environment.GetCommandLineArgs()[0]);
            Configuration config = ConfigurationManager.OpenExeConfiguration(exeCmd);

            ClientSection clients = (ClientSection) config.
                GetSectionGroup("system.serviceModel").Sections["client"];

            ChannelEndpointElement endpoint = clients.Endpoints[0];
            service = new OrchestratorServiceClientProxy(endpoint.Name, endpoint.Address.ToString());
            
            UpdateTrackerStatuses(); 
            
            isInit = true;
            pingTimer = new Timer(this.OnPingTimer, null, -1, Timeout.Infinite);
        }

        /// <summary>
        /// Starts the ping loop which asks the orchestrator for tracker information
        /// once a second.
        /// </summary>
        public void StartPinging()
        {
            pingTimer.Change(1000, Timeout.Infinite);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="envxFileName"></param>
		/// <param name="resultsUri"></param>
		/// <param name="sourceUri"></param>
		/// <param name="friendlyName"></param>
		/// <param name="scenarios"></param>
        public void SubmitJob(string envxFileName, string sourceUri, string resultsUri, string friendlyName, int[] scenarios)
        {
            JobData job = new JobData
            {
                EnvxName = envxFileName,
                ProjectSourceUri = sourceUri,
                FriendlyName = friendlyName,
                ProjectScenarios = scenarios,
				Guid = Guid.NewGuid().ToString()
            };
            SubmitJob(job);
        }

        /// <summary>
        /// Submit a job to the webservice for execution as soon as possible.
        /// </summary>
        /// <param name="job">
        /// The job to be executed.
        /// </param>
        private void SubmitJob(JobData job)
        {
            try
            {
                if (!service.QueueJob(job))
                {
                    logger.Error("error queueing job data: " + job);
                }
            }
            catch (Exception ex)
            {
                logger.Error("The orchestrator seems to have disconnected!", ex);
                throw ex;
            }
        }

        public int NumIdleTrackers()
        {
            return service.NumIdleTrackers();
        }

        public int NumRunningTrackers()
        {
            return service.NumRunningTrackers();
        }

        public int NumWaitingJobs()
        {
            return service.NumWaitingJobs();
        }

        /// <summary>
        /// Get the tracker statuses from the orchestrator endpoint.
        /// </summary>
        private void UpdateTrackerStatuses()
        {
            TrackerData[] updatedTrackerDatas = service.TrackerStatus();
            foreach (TrackerData td in updatedTrackerDatas)
            {
                trackers.AddOrUpdate(Guid.Parse(td.Guid), td, (guid, data) => td);
            }
        }

        /// <summary>
        /// Handles the ping timer expiration event.
        /// </summary>
        /// <param name="state">
        /// The object containing the ping timer.
        /// </param>
        private void OnPingTimer(object state)
        {
            UpdateTrackerStatuses();
            EventHandler<TrackersPingedEventArgs> handler = TrackersPinged;
            if (handler != null)
            {
                handler(this, new TrackersPingedEventArgs(trackers.Values));
            }

            pingTimer.Change(1000, Timeout.Infinite);
        }
    }
}
