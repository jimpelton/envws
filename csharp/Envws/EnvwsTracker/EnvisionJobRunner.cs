using System;
using System.Linq;
using System.IO;

namespace TrackProcess
{
    using EnvwsLib.DataContracts;
    using EnvwsLib.Tracker;
    using EnvwsLib.Util;
    using log4net;

    public class EnvisionJobRunner : JobRunner
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(EnvisionJobRunner));

//        /// <summary>
//        /// Gets or sets the absolute path to the working directory for this JobRunner.
//        /// </summary>
//        private string BaseDir { get; set; }
//
//        /// <summary>
//        /// Gets or sets the absolute path to the remote data directory for this JobRunner.
//        /// </summary>
//        private string RemoteBaseDir { get; set; }

        /// <summary>
        /// Gets or sets the absolute path (including file name) of Envision.exe.
        /// </summary>
        private string EnvExePath { get; set; }

        private string EnvisionOutputDir { get; set; }

        /// <summary>
		/// This projects personal working directory.
        /// </summary>
        private string ProjectWorkingDir { get; set; }

        /// <summary>
        /// The location that Envision output is copied to after a job is run.
        /// </summary>
        private string ResultsDirectory { get; set; }

        private string SourceDirectory { get; set; }

        private string EnvxFilePath { get; set; }

        private string ResultsLogDirectory { get; set; }

        /// <summary>
        /// The filename of envision's log file which will be copied to ResultsLogDirectory/friendly job name.
        /// </summary>
        private string EnvLogFileName { get; set; }

		public EnvisionJobRunner(JobData job) : base(job)
		{
		    ConfigParser parser = ConfigParser.Instance();
            string localBase = parser["BaseDirectory"];
		    string remoteBase = parser["RemoteBaseDirectory"];
		    string remoteResultsDir = parser["ResultsDirectory"];
		    string remoteResultsLogDir = parser["ResultsLogDirectory"];
		    string envOutputDir = parser["EnvisionOutputDirectoryName"];

            string remoteJobNameDir = job.FriendlyName == string.Empty ? job.Guid : job.FriendlyName;
            
            EnvExePath = parser["EnvExePath"];
		    ResultsDirectory = Path.Combine(remoteBase, remoteResultsDir, remoteJobNameDir);
		    ResultsLogDirectory = Path.Combine(remoteBase, remoteResultsLogDir);
            ProjectWorkingDir = Path.Combine(localBase, job.Guid);
            EnvisionOutputDir = Path.Combine(ProjectWorkingDir, envOutputDir);
            EnvxFilePath = Path.Combine(ProjectWorkingDir, job.EnvxName);
            SourceDirectory = job.ProjectSourceUri;
		    
		}

        /// <summary>
        /// Download, unzip, run envision and then re-zip and upload.
        /// Note, the return value only referes to the file operations, 
        /// to find out if the Envision process exited cleanly, check 
        /// <code>job.EnvisionExitCode</code>.
        /// </summary>
        /// <returns>
        /// True if the job was setup correcly and results where uploaded, false otherwise.
        /// </returns>
        //TODO: ExecuteJob return error code.
        public override int ExecuteJob()
        {
            logger.Info(string.Format("Job started: {0} ({1}) ", CurrentJob.FriendlyName, CurrentJob.Guid));

            // Copy the project folder onto the working filesystem.
            CheckForAndCreateDirectory(ProjectWorkingDir);
            RecursivelyCopyEntireDirectory(SourceDirectory, ProjectWorkingDir);
            int exitCode = -1;

            // If the job specifies that certain scenarios should be run, then loop through
            // them, executing each scenario in succession. Otherwise, run the job on
            // every scenario (Envision scenario indexes being at 1, with 0 meaning "all scenarios").
            if (CurrentJob.ProjectScenarios.Length > 0)
            {
                foreach (int scenarioIndex in CurrentJob.ProjectScenarios)
                {
                    exitCode = RunJob(scenarioIndex);
                }
            }
            else
            {
                exitCode = RunJob(0);
            }

            logger.Info(string.Format("Job completed: {0} ({1})", CurrentJob.FriendlyName, CurrentJob.Guid));

            return exitCode;
        }

        /// <summary>
        /// Run a single scenario. Passing 0 for scenarioIndex runs all scenarios.
        /// Once the scenario is done executing, the results are copied into the
        /// results directory.
        /// </summary>
        /// <param name="scenarioIndex">The scenario to run.</param>
        /// <returns>The exit code returned by Envision. -1 is returned if the process failed to start.</returns>
        private int RunJob(int scenarioIndex)
        {
            string envOpts = string.Format("/r:{0}", scenarioIndex);
            ProcessTracker tracker = new ProcessTracker(EnvExePath, envOpts);
            if (!tracker.Start())
            {
                return -1;
            }
            int exitCode = tracker.WaitForCompletion();

            CheckForAndCreateDirectory(ResultsDirectory);
            RecursivelyCopyEntireDirectory(Path.Combine(ProjectWorkingDir, EnvisionOutputDir), 
                ResultsDirectory);
            logger.Info(string.Format("Envision exited. Exit code: {0}", exitCode));

            return exitCode;
        }

        /// <summary>
        /// Sets up directories and checks for the requried files.
        /// </summary>
        /// <returns>true on success, false if something didn't exist.</returns>
        private bool CheckExistingFilesAndCreate()
        {
            bool rval = false;

            // create project directory
            if (CheckForAndCreateDirectory(ProjectWorkingDir))
            {
                logger.Debug(string.Format("Created new directory for source project: {0}", 
                    ProjectWorkingDir));
                rval = true;
            }
            else
            {
                logger.Error(string.Format("Did not create new directory {0} for project {1}. " +
                                           "Either, the directory already exists (from a previous project), or " +
                                           "could not be created because of some other reason.", 
                                           ProjectWorkingDir, CurrentJob.FriendlyName));   
            }

            // check that envx file exists.
            if (!File.Exists(EnvxFilePath))
            {
                logger.Error(string.Format("Envx project file does not exist, skipping job: {0}, {1}", 
                    EnvxFilePath, CurrentJob.Guid));
                rval = false;
            }

            return rval;
        }

        private void CopyEnvLogFile()
        {
            string envLogFilePath = Path.Combine(ProjectWorkingDir, EnvLogFileName);
            CopySingleFile(envLogFilePath, ResultsLogDirectory, EnvLogFileName);
        }



        /// <summary>
        /// Check if a dir exists and create the dir if it does not exist.
        /// Then, check to see if creation was successful.
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns>
        /// true if dir was made, 
        /// false in all other cases (even if it already exists).
        /// </returns>
        private bool CheckForAndCreateDirectory(string dirPath)
        {
            bool rval = false;
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
                if (!Directory.Exists(dirPath))
                {
                     logger.Error(string.Format("Could not create directory: {0}", dirPath));
                }
                rval = true;
            }
            return rval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destPath"></param>
        private void RecursivelyCopyEntireDirectory(string sourceDir, string destPath)
        {    
            new Microsoft.VisualBasic.Devices.Computer().FileSystem.CopyDirectory(sourceDir, destPath);
        }

        /// <summary>
        /// Copy single file, checks all params for invalid path and file characters.
        /// Catches all exceptions that might be thrown and logs them.
        /// </summary>
        /// <param name="sourceFile">Path to the source file.</param>
        /// <param name="destDir">Directory to copy to.</param>
        /// <param name="destFile">Name of the destination file.</param>
        private void CopySingleFile(string sourceFile, string destDir, string destFile)
        {
            try
            {
                // check source file for invalid path chars.
                int i = -1;
                if ((i = sourceFile.IndexOfAny(Path.GetInvalidPathChars())) >= 0)
                {
                    logger.Error(string.Format("The file contains this invalid (filename) character:  {0} {1}", 
                         sourceFile, sourceFile.ElementAt(i)));
                    throw new ArgumentException();
                }
                
                // Combine and GetFileName will check args for invalid chars.
                string path = Path.Combine(destDir, Path.GetFileName(destFile));

                File.Copy(sourceFile, destFile);
                 logger.Info(string.Format("Successfuly copied: {0} to {1}", sourceFile, destFile));
            } 
            catch (Exception e)
            {
                // string f = Path.GetFileName(sourceFile);
                 logger.Error(string.Format("Exception when copying file: {0}", sourceFile), e);
            }
        }

        /// <summary>
        /// Delete a file given by <code>file</code>.
        /// </summary>
        /// <param name="file">
        /// The path to the file to delete.
        /// </param>
        private void DeleteSingleFile(string file)
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception e)
            {
                 logger.Error(string.Format("Exception when deleting file: {0}", file), e);
            }
        }

        /// <summary>
        /// Downloads and unzip's job data.
        /// See setupJobData() for params description.
        /// </summary>
        /// <returns></returns>
        //        private bool DownloadAndUnzip()
        //        {
        //            try
        //            {
        //                // download
        //                using (WebClient client = new WebClient())
        //                {
        //                    logger.Info("Starting transfer: " + CurrentJob.ProjectSourceUri);
        //                    Uri srcUri = new Uri(projectSourceUri);
        //                    Uri zipUri = new Uri(zipFilePath);
        //                    client.DownloadFile(srcUri, zipFilePath);
        //                    logger.Info("Done downloading.");
        //                }
        //
        //                // make unpack-dir (the local working project dir)
        //                if (!Directory.Exists(unpackedDir))
        //                {
        //                    Directory.CreateDirectory(unpackedDir);
        //                    logger.Info("Created directory for unpacked project: " + unpackedDir);
        //                }
        //
        //                // unzip project into dir.
        //                logger.Info("Unzipping file " + zipFilePath + " to " + unpackedDir);
        //                ZipFile.ExtractToDirectory(zipFilePath, unpackedDir);
        //                logger.Info("Done unzipping.");
        //            }
        //            catch (WebException e)
        //            {
        //                logger.Error("Web exception occurred, skipping job. ", e);
        //                return false;
        //            }
        //            catch (Exception e)
        //            {
        //                logger.Error("Exception when downloading and unpacking: ", e);
        //                return false;
        //            }
        //
        //            return true;
        //        }

        /// <summary>
        /// Zip and upload the job results after a run.
        /// </summary>
        /// <param name="projectResultsUri"></param>
        /// <param name="jobGuid"></param>
        /// <param name="unpackedDir"></param>
        /// <param name="zipFileName"></param>
        /// <returns></returns>
        //        private bool ZipAndUpload(string projectResultsUri, string jobGuid, string unpackedDir, string zipFileName)
        //        {
        //            // create zip file.
        //            string zipname = jobGuid + ResultsAppendStr + ".zip";
        //            string zippath = Path.Combine(ProjectWorkingDir, zipname);
        //            try
        //            {
        //                 logger.Info("Copying log files to project directory: " + unpackedDir);
        //                 CopyEnvLogs(unpackedDir);
        //                 logger.Info("Zipping " + zippath);
        //                ZipFile.CreateFromDirectory(unpackedDir, zippath);
        //            }
        //            catch (Exception e)
        //            {
        //                 logger.Error("Failed zipping, skipping upload: ", e);
        //                return false;
        //            }
        //
        //            // upload time.
        //            try
        //            {
        //                using (WebClient client = new WebClient())
        //                {
        //                    string z = Path.Combine(projectResultsUri, zipname);
        //                    Uri zipPath = new Uri(z);
        //                     logger.Info("Starting upload of finished job to " + z);
        //                    client.UploadFile(z, zippath);
        //                }
        //            } 
        //           catch (WebException e)
        //            {
        //                 logger.Error("Trouble uploading the results zip file. ", e);
        //                return false;
        //            }
        //            catch (Exception e)
        //            {
        //                 logger.Error(string.Empty, e);
        //                return false;
        //            }
        //            return true;
        //        }

        //        private void CopyEnvLogs(string unpackedDir)
        //        {
        //            try
        //            {
        //                string envDir = Path.GetDirectoryName(EnvExePath);
        //                string host = Environment.MachineName.ToLower();
        //                string rptlog = ConfigParser.Instance()["envReportLog"].ToLower();
        //                string lbklog = ConfigParser.Instance()["envLogbookLog"].ToLower();
        //                string envlog = ConfigParser.Instance()["envLog"].ToLower();
        //                string logdir = ConfigParser.Instance()["resultsLogDir"];
        //                foreach (string logfile in Directory.GetFiles(envDir, "*.log"))
        //                {
        //                    string log = Path.GetFileName(logfile).ToLower();
        //                    if (log.Contains(host))
        //                    {
        //                        if (log.StartsWith(rptlog))
        //                        {
        //                            rptlog = logfile;
        //                        }
        //                        else if (log.StartsWith(lbklog))
        //                        {
        //                            lbklog = logfile;
        //                        }
        //                        else if (log.StartsWith(envlog))
        //                        {
        //                            envlog = logfile;
        //                        }
        //                    }
        //                }
        //
        //                string projLogDir = Path.Combine(unpackedDir, logdir);
        //                 CheckForAndCreateDirectory(projLogDir);
        //
        //                // copy original logs to project directory.
        //                 CopySingleFile(rptlog, projLogDir, rptlog);
        //                 CopySingleFile(lbklog, projLogDir, lbklog);
        //                 CopySingleFile(envlog, projLogDir, envlog);
        //
        //                // delete original logs
        //                 DeleteSingleFile(rptlog);
        //                 DeleteSingleFile(lbklog);
        //                 DeleteSingleFile(envlog);
        //
        //            }
        //            catch (Exception e)
        //            {
        //                string msg = string.Empty;
        //                if (e is NullReferenceException)
        //                {
        //                    msg =
        //                    "NullReferenceException when copying logs: " +
        //                    "Are all log paths and directories specified in the tracker configuration file?";
        //                }
        //                 logger.Error(string.Format("Exception in copyEnvLogs: {0}", msg), e);
        //            }
        //        }
    }
}
