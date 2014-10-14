using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvwsLib.DataContracts;
using EnvwsLib.Util;
using EnvwsTracker;
using log4net.Config;

namespace EnvwsTracker
{
    using log4net;

    class Program
    {
        private static ILog logger;

        public static void Main(string[] args)
        {
            logger = LogManager.GetLogger(typeof(TrackProcessClient));

            ConfigParser config = ConfigParser.Instance();
            config.AddOpt(ConfigOpts.Get(ConfigKey.BaseDirectory))
                .AddOpt(ConfigOpts.Get(ConfigKey.EnvExePath))
                .AddOpt(ConfigOpts.Get(ConfigKey.EnvisionOutputDirectoryName))
                .AddOpt(ConfigOpts.Get(ConfigKey.EnvLog))
                .AddOpt(ConfigOpts.Get(ConfigKey.RemoteBaseDirectory))
                .AddOpt(ConfigOpts.Get(ConfigKey.ResultsLogDirectory))
                .AddOpt(ConfigOpts.Get(ConfigKey.ResultsDirectory))
                .AddOpt(ConfigOpts.Get(ConfigKey.Log4NetConfigFile))
                .SetDefaultOptValue(ConfigOpts.Get(ConfigKey.Log4NetConfigFile), "log4.config")
                .SetDefaultOptValue(ConfigOpts.Get(ConfigKey.EnvisionOutputDirectoryName), "Outputs");

            ParseArgsAndOpenConfigFile(args, config);

            // setup Log4Net
            string configFile = config[ConfigOpts.Get(ConfigKey.Log4NetConfigFile)].Value;
            if (!File.Exists(configFile))
            {
                BasicConfigurator.Configure();
                Console.WriteLine("Logger configured with BasicConfigurator because " +
                                  "config file {0} was not found.", configFile);
            }
            else
            {
                XmlConfigurator.Configure(new FileInfo(configFile));
                Console.WriteLine("Logger configured with config file: {0}", configFile);
            }

            // check that we have every option required
            if (!CheckRequiredConfigOptions(config))
            {
                logger.Fatal("Some configuration options did not pass checks."
                    + "Check config file for any incorrect values.");
                Environment.Exit(0);
            }

            TrackProcessClient client = new TrackProcessClient(
                new TrackerData()
                {
                    Guid = Guid.NewGuid().ToString(),
                    Status = TrackerStatus.IDLE,
                    HostName = Environment.MachineName
                });

            if (client.StartManager())
            {
                logger.Info("Found Orchestrator.");
                client.StartPingLoop();
                logger.Info("Tracker is ready.");
                Console.WriteLine("Press <return> to exit...");
                Console.ReadLine();
            }
            else
            {
                logger.Fatal("An orchestrator was not found. Exiting.");
                Console.Error.WriteLine("An orchestrator was not found. Exiting.");
                Environment.Exit(0);
            }
        }

        // check for config options that must exist for sure, return
        // false if one check does not pass, but still checks everything.
        private static bool CheckRequiredConfigOptions(ConfigParser config)
        {
            bool ok = true;

            string envexe = config[ConfigOpts.Get(ConfigKey.EnvExePath)].Value;
            if (!File.Exists(envexe))
            {
                // This is not a fatal error, so ok still = true.
                logger.Warn(String.Format("Envision executable not found: {0}", envexe));
            }

            string workingDir = config[ConfigOpts.Get(ConfigKey.BaseDirectory)].Value;
            if (!Directory.Exists(workingDir))
            {
                // The working directory must already exist
                logger.Fatal(String.Format("Working dir not found: {0}", workingDir));
                ok = false;
            }

            return ok;
        }

        // check command line args, and for config file.
        private static void ParseArgsAndOpenConfigFile(string[] args, ConfigParser Config)
        {
            if (args.Length >= 1)
            {
                if (args[0].ToLower() == "-v")
                {
                    Console.WriteLine("Version: {0}", RepoVer.VER);
                    Environment.Exit(0);
                }

                string configFile = args[0];
                if (File.Exists(configFile))
                {
                    Config.Open(configFile);
                    logger.Info(Config.GetFormatedOptionsString());
                }
                else
                {
                    logger.Fatal(string.Format("Config file {0} doesn't exist. Exiting...", configFile));
                    Console.WriteLine("Config file {0} doesn't exist. Exiting...", configFile);
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
    }
}
