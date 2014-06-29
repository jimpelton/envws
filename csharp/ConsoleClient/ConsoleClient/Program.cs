using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleClient
{
    using ClientLib;
    using EnvwsLib.DataContracts; 

    public class Command
    {
        public string Name { get; set; }
        public string Args { get; set; }
        public string Cmd { get; set; }
    }

    public class Program
    {

        private static Dictionary<string, Command> Commands =
            new Dictionary<string, Command>() 
            {
                {"ls", 
                    new Command { Name="list-trackers", Args="No arguments.", Cmd="ls"} },
                {"s", 
                    new Command { Name="submit-job", Args="<job name> <envx name> <source uri> <results uri> <scenarios[]>", Cmd="s"} }
            };

		private static readonly List<string> ShortCommands = 
            new List<string>() 
			{
				"ls", "s"
            };

        public static void Main(string[] args)
        {
			// check if command is -v (print version information)
			if (args.Length == 1)
            {
				if (args[0].ToLower() == "-v")
				{
					Console.WriteLine("Version: " + RepoVer.VER);
					Environment.Exit(0);
				}
            }

			// args[0]=<orch-endpoint> args[1]=<command> args[2..n]=optional args
            if (args.Length < 2) 
            { 
                printUsageAndExit(); 
            }
            
            string orchEndpoint = args[0];
            string command = args[1];

			// commands start with a leading '-'
            if (!command.StartsWith("-")) 
            { 
                printUsageAndExit(); 
            }

			command = command.Substring(1);

			// check that command is actually a command
			if (!Commands.ContainsKey(command)) 
            { 
                printUsageAndExit(); 
            }

			string[] commandArgs;

			// copy optional command arguments
            if (args.Length > 2)
            {   
                // copy everything after the command
                commandArgs = new string[args.Length - 2];
                Array.Copy(args, 2, commandArgs, 0, commandArgs.Length);
            }
            else 
            {  
                commandArgs = new string[0]; 
            }

			// print command specific help
            if (commandArgs.Length >= 1 && commandArgs[0].StartsWith("h")) 
            { 
                printUsageAndExit(command); 
            }

            ConsoleClient cc = new ConsoleClient();

            if (command == "ls")
                DoTrackerDatas(cc);
            else if (command == "s")
                DoSubmitJob(cc, commandArgs);
        }

        private static void DoTrackerDatas(ConsoleClient cc)
        {
            cc.Init();
            List<TrackerData> data = new List<TrackerData>(cc.GetTrackerData());
            if (data.Count == 0)
            {
                Console.WriteLine("No Trackers");
            }
            else
            {
                int running = data.Count(td => td.Status == TrackerStatus.RUNNING);
                int down = data.Count(td => td.Status == TrackerStatus.NO_RESPONSE || td.Status == TrackerStatus.UNKNOWN);
                int idle = data.Count(td => td.Status == TrackerStatus.IDLE);
				
                Console.WriteLine(data.Count + " registered trackers.");
				Console.WriteLine(running + " running.");
				Console.WriteLine(down + " down.");
				Console.WriteLine(idle+ " idle.");

                foreach (TrackerData td in data)
                {
                    string jobName = td.CurrentJob.FriendlyName == string.Empty ? td.CurrentJob.Guid : td.CurrentJob.FriendlyName;
					string msg = string.Format("{0} {1}\t{2}", td.HostName, td.Status, jobName);
                    Console.WriteLine(msg);
                }
            }
        }
        
        private static void DoJobList(ConsoleClient cc)
        {
            cc.Init();
            List<TrackerData> data = new List<TrackerData>(cc.GetTrackerData());
            if (data.Count==0)
            {
                Console.WriteLine("No Jobs.");
            }
            else
            {
                
            }
        }

        private static void DoSubmitJob(ConsoleClient cc, string[] cmdArgs)
        {
            if (cmdArgs.Length <= 3)
            {
                usage(Commands["s"].Cmd);
            }
            else
            {
                string friendlyName = cmdArgs[0];
                string envxName = cmdArgs[1];
                string sourceUri = cmdArgs[2];
                string resultsUri = cmdArgs[3];
                int[] scenarios;

                if (cmdArgs.Length <= 4)
                {
                    scenarios = new int[] { 0 };
                }
                else
                {
                    string scenariosString = cmdArgs[4];
                    scenarios = scenariosString.Split(',')
                        .Select(numStr => 
							{ 
								int a; 
								Int32.TryParse(numStr, out a); 
								return a; 
							}
                        ).ToArray();
                }
                cc.Init();
                cc.SubmitJob(envxName, sourceUri, resultsUri, friendlyName, scenarios);
            }
        }

        private static void printUsageAndExit()
        {
            usage("");
            Environment.Exit(0);
        }
        
        private static void printUsageAndExit(String cmd)
        {
            usage(cmd);
            Environment.Exit(0);
        }

        private static void usage(String cmd)
        {
            string msg = 
                "Usage: \n" +
				"\t<endpoint> <command> [command-options]\n\n";

            if (cmd == string.Empty)
            {
                msg += validCommandsUsageString();
            }
            else if (Commands.ContainsKey(cmd))
            {
                msg = "Usage for " + Commands[cmd].Name + ": -" + Commands[cmd].Cmd + " " + Commands[cmd].Args;
            }
            else
            {
                msg += "Unknown command.\n" + validCommandsUsageString();
            }

            Console.WriteLine(msg);
        }

        private static void usage()
        {
            usage("");
        }

        private static string validCommandsUsageString()
        {
            string msg = "\tValid Commands: \n";
            foreach (Command c in Commands.Values)
            {
                msg += "\t" + c.Name + ":\t-" + c.Cmd + " " + c.Args + "\n";
            }
            return msg;
        }
    }
}
