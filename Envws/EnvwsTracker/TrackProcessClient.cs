
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
    using System.Collections.Generic;

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
        private static int RequestJobFreqMillis { get; set; }
        private static int CheckInFreqMillis { get; set; }

        private static IList<JobData> completedJobs = new List<JobData>();
        private static JobData JobToBeReturned = null;

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
            ReturnFinishedJob(args.Job);
            RequestNewJob(this);
        }

        /// <summary>
        /// Returns the given job.
        /// </summary>
        /// <returns>False if the Orchestrator's endpoint was not found, true otherwise.</returns>
        private bool ReturnFinishedJob(JobData job)
        {
            bool rval = false;
            try
            {
                if (JobNeedsReturning)
                {

                }
                Client.ReturnFinishedJob(job);
                JobToBeReturned = null;
                JobNeedsReturning = false;
                rval = true;
            }
            catch (EndpointNotFoundException e)
            {
                logger.Error("Orchestrator not found when returning new job.", e);
                RequestJobFreqMillis = 5000;
                JobNeedsReturning = true;
                JobToBeReturned = job;
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
                    jd.TrackerGuid = data.Guid;
                    manager.AddNewJob(jd);
                    logger.Info("Added new job to manager.");

                    // stop job checkouts until we request another job.
                    RequestJobFreqMillis = 0;
                }
                else
                {
                    RequestJobFreqMillis = 1000;
                    logger.Debug("No new jobs on orchestrator, checking back in one second.");
                }
            } 
            catch (EndpointNotFoundException e)
            {
                logger.Error("Orchestrator not found when requesting new job.", e);
                RequestJobFreqMillis = 5000;
            }
            finally
            {
                checkJobTimer.Change(RequestJobFreqMillis, Timeout.Infinite);
            }
        }

        private void OnPingTimer(object state)
        {
            DoCheckIn();
            pingTimer.Change(CheckInFreqMillis, Timeout.Infinite);
        }

        private void StartPingLoop()
        {
            pingTimer.Change(CheckInFreqMillis, Timeout.Infinite);
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


        public bool JobNeedsReturning { get; set; }
    } // TrackProcessClient
}
