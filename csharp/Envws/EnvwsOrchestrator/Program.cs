namespace EnvwsOrchestrator
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Discovery;
    using System.ServiceProcess;

    using log4net;
    using log4net.Config;

    public class Program /*: ServiceBase*/
    {
        private static ILog logger = LogManager.GetLogger(typeof(Program));

        private ServiceHost trackerHost = null;
        private ServiceHost clientHost  = null;

        public static void Main(string[] args)
        {
            string cmd = string.Empty;	
			if (args.Length >= 1)
            {
                cmd = args[0];
				if (cmd == "-v")
                {
                    Console.WriteLine("Version: " + EnvwsOrchestrator.RepoVer.VER);
                    Environment.Exit(0);
                }
            }

            XmlConfigurator.Configure(new System.IO.FileInfo("log4.config"));

            Program p = new Program();
            p.Start();
            Console.WriteLine("Press <Enter> to quit...");
            Console.ReadLine();
            p.Stop();
            Console.WriteLine("Exiting...");
        }

        public Program()
        {
            //this.ServiceName = "OrchestratorServices";
        }

        public void Start()
        {
            if (trackerHost != null)
            {
                trackerHost.Close();
            }

            if (clientHost != null)
            {
                clientHost.Close();
            }

            trackerHost = new ServiceHost(typeof(CheckInService));
            clientHost = new ServiceHost(typeof(OrchestratorService));

            trackerHost.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
            trackerHost.AddServiceEndpoint(new UdpDiscoveryEndpoint());

            const int fiveSeconds = 5000;
            TrackerQueue tq = new TrackerQueue(fiveSeconds);
            CheckInService.Q = tq;
            OrchestratorService.TrackerQueue = tq;
            
            trackerHost.Open();
            clientHost.Open();
            OrchestratorService.TrackerQueue.StartScrubLoop();

            logger.Info("Orchestrator service is ready.");
        }

        public void Stop()
        {
            if (trackerHost != null) 
            { 
                trackerHost.Close();
                trackerHost = null;
                logger.Info("CheckIn service stopped.");
            }

            if (clientHost != null)
            {
                clientHost.Close();
                clientHost = null;
                logger.Info("Client service stopped.");
            }
            
            logger.Info("Services stopped.");
        }

        public void Printendpoints(ServiceEndpointCollection sec)
        {
            Console.WriteLine("Listening on: ");
            foreach (ServiceEndpoint s in sec)
            {
                logger.Info(s.Address.Uri.ToString());
            }
        }
    }
}