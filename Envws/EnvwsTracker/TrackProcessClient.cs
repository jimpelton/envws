
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Threading;
using System.Linq;
using System.Linq.Expressions;

using log4net;
using log4net.Config;

namespace EnvwsTracker
{
    using EnvwsLib;
    using EnvwsLib.Tracker;
    using EnvwsLib.Util;
    using EnvwsLib.DataContracts;
    using EnvwsLib.ServiceProxies;
    using EnvwsLib.ServiceContracts;
    using System.Collections.Generic;

	/// <summary>
	/// TrackProcessClient is a client to the Orchestrator service.
	/// </summary>
    public class TrackProcessClient
    {
        
        private static ILog logger;

        private TrackerData Data;

        private ProcessTrackerManager Manager { get; set; }

        private Timer PingTimer;

        private Timer CheckJobTimer;

        private string MachineName = string.Empty;

        private int RequestJobFreqMillis { get; set; }

        private int CheckInFreqMillis { get; set; }

        private const int CHECKIN_ONE_SECOND = 1000;
        private const int CHECKIN_FIVE_SECOND = 5000;

        private IList<TrackerJobData> CompletedJobs = new List<TrackerJobData>();

        public TrackProcessClient(TrackerData td /*, CheckInServiceClient client*/ )
        {
            Data = td;
            RequestJobFreqMillis = CHECKIN_ONE_SECOND;
            CheckInFreqMillis = CHECKIN_ONE_SECOND;
        }

        public static ConfigParser Config { get; private set; }

        private CheckInServiceClientProxy Client { get; set; }
        
        /// <summary>
        /// Starts the ProcessTrackerManager for this process tracker. This method
        /// blocks until the orchestrator is automatically found (or a timeout occurs),
        /// then the ping loop and job request loops are started.
        /// This method requests a new job before it returns.
        /// </summary>
        /// <returns>
        /// False if the orchestrator was not found, true otherwise.
        /// </returns>
        public bool StartManager()
        {
            Client = InvokeCheckInService(FindCheckInService());
            if (Client != null)
            {
                PingTimer = new Timer(this.OnPingTimer, this, Timeout.Infinite, Timeout.Infinite);
                CheckJobTimer = new Timer(this.RequestNewJob, this, Timeout.Infinite, Timeout.Infinite);
                //this.StartPingLoop();
            }
            else
            {
				// An orchestrator was not found.
                return false;
            }

            Manager = new ProcessTrackerManager()
            {
                WorkingDir = Config["workingDir"],
                ResultsAppendStr = Config["resultsAppendStr"],
                EnvExePath = Config["envExePath"]
            };

            Manager.JobCompleted += this.OnJobComplete;
            Manager.Start();

            RequestNewJob(this);
            return true;
        }

		/// <summary>
		/// Sends the CheckIn message and submits the current tracker data.
		/// </summary>
		/// <returns>
        /// False if the check in failed for some reason, true otherwise.
        /// </returns>
        public bool DoCheckIn()
        {
            bool rval = false;
            try
            {
                Data.Status = Manager.Status;
                Client.CheckIn(Data);
                rval = true;

                logger.Debug("Checked in.");
            }
            catch (EndpointNotFoundException e)
            {
                logger.Error("Orchestrator not found when checking in.", e);
            }

            return rval;
        }

		/// <summary>
		/// Handle an OnJobCompleteEvent, sent by the ProcessTrackerManager instance.
        /// Returns the finished jobdata object contained in <code>args</code>, then
        /// requests a new job.
		/// </summary>
        private void OnJobComplete(object sender, JobCompletedEventArgs args)
        {
			CompletedJobs.Add(
                new TrackerJobData() 
				{
                    TheJobInQuestion = args.Job,
                    HasBeenReturnedToTracker = false
                });

            ReturnFinishedJobs(args.Job);
            RequestNewJob(this);
        }

        /// <summary>
        /// Returns the given job.
        /// </summary>
        /// <returns>False if the Orchestrator's endpoint was not found, true otherwise.</returns>
        private bool ReturnFinishedJobs(JobData job)
        {
            bool rval = false;
            try
            {
				IEnumerable<TrackerJobData> unreturnedJobs = 
                    CompletedJobs.Where<TrackerJobData>(j => !j.HasBeenReturnedToTracker);
				
				foreach (TrackerJobData j in unreturnedJobs)
				{
                    Client.ReturnFinishedJob(j.TheJobInQuestion);					
                }

                rval = true;
                RequestJobFreqMillis = CHECKIN_ONE_SECOND;
            }
            catch (EndpointNotFoundException e)
            {
                logger.Error("Orchestrator not found when returning new job.", e);
                RequestJobFreqMillis = CHECKIN_FIVE_SECOND;
            }
            return rval;
        }

		/// <summary>
		/// Requests a new job from the tracker.
		/// </summary>
        private void RequestNewJob(object state)
        {
            JobData jd;
            try
            {
                jd = Client.RequestJob();
                if (jd != null)
                {
                    logger.Info("Got new job.");
                    jd.TrackerGuid = Data.Guid;

					//TODO: check thread safety.
                    Manager.AddNewJob(jd);
                    logger.Info("Added new job to manager.");

                    // stop job checkouts until we request another job.
                    RequestJobFreqMillis = Timeout.Infinite;
                }
                else
                {
                    RequestJobFreqMillis = CHECKIN_ONE_SECOND;
                    logger.Debug("No new jobs on orchestrator, checking back in one second.");
                }
            } 
            catch (EndpointNotFoundException e)
            {
                logger.Error("Orchestrator not found when requesting new job.", e);
                RequestJobFreqMillis = CHECKIN_FIVE_SECOND;
            }
            finally
            {
                CheckJobTimer.Change(RequestJobFreqMillis, Timeout.Infinite);
            }
        }

        private void OnPingTimer(object state)
        {
            DoCheckIn();
            PingTimer.Change(CheckInFreqMillis, Timeout.Infinite);
        }

        private void StartPingLoop()
        {
            PingTimer.Change(CheckInFreqMillis, Timeout.Infinite);
            logger.Info("Starting ping loop.");
        }

		/// <summary>
		/// Searches for the orchestrator checkin service.
		/// </summary>
		/// <returns>The EndpointAddress of the first checkinservice found, otherwise null.</returns>
        private EndpointAddress FindCheckInService()
        {
            DiscoveryClient discoveryClient = 
                new DiscoveryClient(new UdpDiscoveryEndpoint());
            
            FindResponse findResponse = 
                discoveryClient.Find(new FindCriteria(typeof(ICheckInService)));

            EndpointAddress rval = null;

            if (findResponse.Endpoints.Count > 0)
            {
                rval = findResponse.Endpoints[0].Address;
            }

            return rval;
        }

		/// <summary>
		/// Connect to the service at <code>endpointAddress</code>.
		/// </summary>
        private CheckInServiceClientProxy InvokeCheckInService(EndpointAddress endpointAddress)
        {
            if (endpointAddress == null)
            {
                return null;
            }

            CheckInServiceClientProxy client 
                = new CheckInServiceClientProxy(new BasicHttpBinding(), endpointAddress);

            return client;
        }

        public static void Main(string[] args)
        {
//            BasicConfigurator.Configure();
            logger = LogManager.GetLogger(typeof(TrackProcessClient));

            Config = ConfigParser.Instance();
            Config.AddOpt("workingDir")
                  .AddOpt("resultsAppendStr")
                  .AddOpt("envExePath")
                  .AddOpt("envReportLog")
                  .AddOpt("envLogbookLog")
                  .AddOpt("envLog")
                  .AddOpt("resultsLogDir")
                  .AddOpt("log4NetConfigFile")
                  .SetDefaultOptValue("log4NetConfigFile", "log4.config");

            ParseArgs(args);

            TrackerData td = new TrackerData()
            {
                Guid = Guid.NewGuid().ToString(),
                Status = TrackerStatus.IDLE,
                HostName = Environment.MachineName
            };

            string configFile = Config["log4NetConfigFile"];
			if (!File.Exists(configFile))
            {
                BasicConfigurator.Configure();
            }
			
            XmlConfigurator.Configure(new System.IO.FileInfo(configFile));

            TrackProcessClient client = new TrackProcessClient(td);
            if (client.StartManager())
            {
                client.StartPingLoop();
                logger.Info("Found Orchestrator at: ");
                logger.Info("Tracker is ready.");
                Console.WriteLine("Press <return> to exit...");
                Console.ReadLine();
            }
        }

        private static void ParseArgs(string[] args)
        {
            if (args.Length >= 1)
            {
				if (args[0].ToLower() == "-v")
                {
                    Console.WriteLine("Version: " + EnvwsTracker.RepoVer.VER);
                    Environment.Exit(0);
                }
                string configFile = args[0];
                if (File.Exists(configFile))
                {
                    Config.Open(configFile);
                }
                else
                {
                    logger.Error("Config file " + configFile + " doesn't exist. Exiting...");
                    Console.WriteLine("Config file " + configFile + " doesn't exist. Exiting...");
                    Usage();
                    Environment.Exit(0);
                }
            }
            else
            {
                Usage();
                Environment.Exit(0);
            }
        }

        private static void Usage()
        {
            Console.WriteLine("Usage: TrackProcessService.exe <config-file>");
        }
    } // TrackProcessClient
}
