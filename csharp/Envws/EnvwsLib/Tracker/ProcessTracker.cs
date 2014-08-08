using System;
using System.Diagnostics;
using System.Threading;

namespace EnvwsLib.Tracker
{
    using log4net;

    /// <summary>
    /// Provides a way to block a thread until a process has completed. 
    ///  
    /// Give a ProcessTracker an executable path and arguments to the process.
    /// Once Start() is called and returns any thread may call WaitForCompletion()
    /// which simply blocks until the process has exited. WaitForCompletion() returns 
    /// the exit code of the process.
    /// </summary>
    public class ProcessTracker
    {
        private static ILog m_logger = 
            LogManager.GetLogger(typeof(ProcessTracker));

        private AutoResetEvent m_resetEvent = new AutoResetEvent(false);
        
        public string ProcessPath{ get; private set; }

        public string ProcessArgs{ get; private set; }

        public int ExitCode { get; private set; }
       
        /// <summary>
        /// Creates a new ProcessTracker.
        /// </summary>
        /// <param name="processPath">Path to executable.</param>
        /// <param name="processArgs">Path to space-separated arguments.</param>
        public ProcessTracker(string processPath, string processArgs)
        {
            ProcessPath = processPath.Trim();
            ProcessArgs = processArgs.Trim();
            ExitCode = 0;
        }

        /// <summary>
        /// Start the process. After this is called your process should be executed and you can
        /// wait for it to finish by calling the blocking method WaitForCompletion().
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ComponentModel.Win32Exception"></exception>
        /// <exception cref="System.ObjectDisposedException"></exception>
        /// <exception cref="System.IO.FileNotFoundDescription">If the executable isn't found.</exception>
        public bool Start() 
        {
			Process proc = null;
			m_logger.Debug(string.Format("Starting process: {0}", ProcessPath));
            proc = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo(ProcessPath, ProcessArgs)
            };
                
            proc.Exited += (sender, e) =>
			{
				ExitCode = proc.ExitCode;
				m_logger.Debug(string.Format("Process ended with exit code: {0}", ExitCode));
				if (proc != null) proc.Close();
				m_resetEvent.Set();
			};
            
            bool ok = false;

            try
            {

                ok = proc.Start();
                if (!ok)
                {
                    m_logger.Error("Process was not started.");
                    ExitCode = -1;
                    m_resetEvent.Set();
                }
                else
                {
                    m_logger.Debug("Process successfully started.");
                }
            }
            catch (Exception e)
            {
                string msg = string.Format("An exception was thrown (and caught) " +
                                           "when starting process {0}", ProcessPath);
                m_logger.Error(msg, e);
                m_resetEvent.Set();
                ExitCode = -1;
                ok = false;
            }
            return ok;
        }

        /// <summary>
        /// Blocks until the process has completed.
        /// </summary>
        /// <returns>The exit code of the process.</returns>
        public int WaitForCompletion()
        {
            m_resetEvent.WaitOne();
            return ExitCode;
        }
    }
}
