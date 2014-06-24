
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Threading;

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

	/// <summary>
	/// TrackProcessClient is a client to the Orchestrator service.
	/// </summary>
    public class TrackProcessClient
    {
        
        private static ILog logger;

        private static TrackerData data;
        private static ProcessTrackerManager manager;

        private static Timer pingTimer;
        private static Timer checkJobTimer;
        private static string machineName = string.Empty;
        
        public TrackProcessClient(TrackerData td /*, CheckInServiceClient client*/ )
        {
            data = td;
        }

        public static ConfigParser Config { get; private set; }

        private static CheckInServiceClientProxy Client { get; set; }
        
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
                pingTimer = new Timer(this.OnPingTimer, this, Timeout.Infinite, Timeout.Infinite);
                checkJobTimer = new Timer(this.RequestNewJob, this, Timeout.Infinite, Timeout.Infinite);
                this.StartPingLoop();
            }
            else
            {
                return false;
            }

            manager = new ProcessTrackerManager()
            {
                WorkingDir = Config["workingDir"],
                ResultsAppendStr = Config["resultsAppendStr"],
                EnvExePath = Config["envExePath"]
            };

            manager.JobCompleted += this.OnJobComplete;
            manager.Start();

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
                data.Status = manager.Status;
                Client.CheckIn(data);
                rval = true;

                logger.Debug("Checked in.");
            }
            catch (Exception e)
            {
                logger.Debug("Woops! CheckIn failed that time. I'll keep trying!", e);
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
            Client.ReturnFinishedJob(args.JobData);
            RequestNewJob(this);
        }

		/// <summary>
		/// Requests a new job from the tracker.
		/// </summary>
        private void RequestNewJob(object state)
        {
            JobData jd = Client.RequestJob();
            if (jd != null)
            {
                logger.Debug("Got new job.");
                jd.TrackerGuid = data.Guid;
                manager.AddNewJob(jd);

                logger.Debug("Added new job to manager.");
            }
            else
            {
                checkJobTimer.Change(1000, Timeout.Infinite);
                logger.Debug("No new jobs on orchestrator, checking back in one second.");
            }
        }

        private void OnPingTimer(object state)
        {
            DoCheckIn();
            pingTimer.Change(1000, Timeout.Infinite);
        }

        private void StartPingLoop()
        {
            pingTimer.Change(1000, Timeout.Infinite);
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
            BasicConfigurator.Configure();
            logger = LogManager.GetLogger(typeof(TrackProcessClient));

            Config = ConfigParser.Instance();
            Config.AddOpt("workingDir")
                  .AddOpt("resultsAppendStr")
                  .AddOpt("envExePath")
                  .AddOpt("envReportLog")
                  .AddOpt("envLogbookLog")
                  .AddOpt("envLog")
                  .AddOpt("resultsLogDir");

            ParseArgs(args);
            TrackerData td = new TrackerData()
            {
                Guid = Guid.NewGuid().ToString(),
                Status = TrackerStatus.IDLE,
                Uri = Environment.MachineName
            };

            TrackProcessClient client = new TrackProcessClient(td);
            if (client.StartManager())
            {
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
