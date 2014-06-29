using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EnvwsLib.Tracker;

namespace EnvwsTracker
{
    using log4net;

    using EnvwsLib;
    using EnvwsLib.Util;
    using EnvwsLib.DataContracts;

    public class ProcessTrackerManager
    {
        /// <summary>
        /// Gets a reference to the JobData of the currently running job.
        /// </summary>
        public JobData CurrentJob
        {
            get
            {
                lock (this.currJobMutex)
                {
                    return this.currentJob;
                }
            }

            private set
            {
                lock (this.currJobMutex)
                {
                    this.currentJob = value;
                }
            }
        }

        /// <summary>
        /// Gets a reference to this trackers current status (RUNNING, IDLE, etc).
        /// </summary>
        public TrackerStatus Status
        {
            get
            {
                lock (this.statusMutex)
                {
                    return this.status;
                }
            }

            private set
            {
                lock (this.statusMutex)
                {
                    this.status = value;
                }
            }
        }

        private readonly ILog logger = LogManager.GetLogger(typeof(ProcessTrackerManager));
        
        private readonly WFQueue<JobData> jobqueue = new WFQueue<JobData>();

        private Task watchTask;

        private JobData currentJob;
        private readonly object currJobMutex = new object();

        private bool keepWorking = false;
        private readonly object keepWorkingMutex = new object();

        private TrackerStatus status = TrackerStatus.UNKNOWN;
        private readonly object statusMutex = new object();

        public event EventHandler<JobCompletedEventArgs> JobCompleted;

        /// <summary>
        /// Gets or sets the absolute path (including file name) of Envision.exe.
        /// </summary>
        public string EnvExePath { get; set; }

        /// <summary>
        /// Gets or sets a directory on the local filesystem where all jobs are unzipped.
        /// Envision will write to the project subdirectories in this path.
        /// </summary>
        public string WorkingDir { get; set; }

        /// <summary>
        /// Gets or sets the string that is appended to the project results zip file.
        /// </summary>
        public string ResultsAppendStr { get; set; }

        private bool KeepWorking
        {
            get
            {
                lock (this.keepWorkingMutex)
                {
                    return this.keepWorking;
                }
            }

            set
            {
                lock (this.keepWorkingMutex)
                {
                    this.keepWorking = value;
                }
            }
        }

        /// <summary>
        /// Adds a new job to the job queue.
        /// </summary>
        /// <param name="job">
        /// The job to add.
        /// </param>
        public void AddNewJob(JobData job)
        {
            this.jobqueue.EnqueueWaiting(job);
            this.Status = TrackerStatus.RUNNING;
        }

        /// <summary>
        /// Returns the size of the waiting job queue.
        /// </summary>
        /// <returns>
        /// An int that is the number of jobs waiting in the job queue.
        /// </returns>
        public int NumWaiting()
        {
            return this.jobqueue.SizeWaiting();
        }

        /// <summary>
        /// Start the watch task and job processing loop.
        /// </summary>
        public void Start()
        {
            if (!this.KeepWorking)
            {
                this.KeepWorking = true;
                this.watchTask = new Task(this.Watch);
                this.watchTask.Start();
            }
        }

        /// <summary>
        /// Called when a job is completed, and calls the user supplied EventHandler function.
        /// </summary>
        /// <param name="e">
        /// Ya know.
        /// </param>
        protected virtual void OnJobCompleted(JobCompletedEventArgs e)
        {
            EventHandler<JobCompletedEventArgs> handler = JobCompleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Watch for a new job, then execute it.
        /// </summary>
        private void Watch()
        {
            this.logger.Info("Watch loop started.");
            this.Status = TrackerStatus.IDLE;
            while (this.KeepWorking)
            {
                this.logger.Info("Waiting for new job.");
                JobData job = this.jobqueue.DequeueWaiting();
                this.Status = TrackerStatus.RUNNING;

                this.logger.Info("New job dequeued: " + job.Guid);
                this.logger.Info("Starting job." + job.Guid);
                
                CurrentJob = job;
                job.StartTime = Utils.CurrentUTCMillies();
                bool rval = this.ExecuteJob(job);
                this.logger.Info("executeJob() returned: " + rval);
                job.FinishTime = Utils.CurrentUTCMillies();
                CurrentJob = null;
                this.jobqueue.EnqueueDone(job);
                this.Status = TrackerStatus.IDLE;
                this.logger.Info("Job finished." + job.Guid);

                //send job completed event
                JobCompletedEventArgs args = new JobCompletedEventArgs { Job = job };
                OnJobCompleted(args);
            }
            this.logger.Info("Watch loop exiting.");
        }

        /// <summary>
        /// Download, unzip, run envision and then re-zip and upload.
        /// Note, the return value only referes to the file operations, 
        /// to find out if the Envision process exited cleanly, check 
        /// <code>job.EnvisionExitCode</code>.
        /// </summary>
        /// <param name="job">
        /// The JobData describing the job to run.
        /// </param>
        /// <returns>
        /// True if the job was setup correcly and results where uploaded, false otherwise.
        /// </returns>
        private bool ExecuteJob(JobData job)
        {
            this.logger.Info("Job " + job.Guid + " started.");

            string projectDir = Path.Combine(WorkingDir, job.Guid);             // workingdir\guid
            string zipFileName = Path.GetFileName(job.ProjectSourceUri);        // proj.zip
            string zipFilePath = Path.Combine(projectDir, zipFileName);         // workingdir\guid\proj.zip
            string unpackedDir = Path.Combine(projectDir, "unpacked");          // workingdir\guid\unpacked

            string envxFilePath = Path.Combine(unpackedDir, job.EnvxName);      // workingdir\guid\proj.envx
            
            // "/r:0" is an Env cl arg meaning run all scenarios
            string envOpts = envxFilePath + " /r:0";

            bool rval = this.SetupJobData(job, envxFilePath, projectDir, zipFilePath, unpackedDir);
            if (!rval)
            {
                return false;
            }

            ProcessTracker tracker = new ProcessTracker(EnvExePath, envOpts);
            tracker.Start();
            int exitCode = tracker.WaitForCompletion();
            job.EnvisionExitCode = exitCode;
            this.logger.Info("Envision exited. Exit code: " + exitCode);
            this.logger.Info("Begining upload: " + job.ProjectResultsUri);

            rval = this.ZipAndUpload(job.ProjectResultsUri, job.Guid, unpackedDir, zipFileName);
            if (!rval)
            {
                return false;
            }

            this.logger.Info("Job " + job.Guid + " completed.");
            return true;
        }

        /// <summary>
        /// Sets up directories, downloads and unpacks data.
        /// </summary>
        /// <param name="job">the job that is being setup.</param>
        /// <param name="envxFilePath">the absolute path to the .envx file</param>
        /// <param name="projectDir">A directory to unpack to (WorkingDir/Guid)</param>
        /// <param name="zipFilePath">the local absolute path that the project zip will be d/l'ed to.</param>
        /// <param name="unpackedDir"> the directory that the zip file will be unpacked to.</param>
        /// <returns></returns>
        private bool SetupJobData(
            JobData job,
            string envxFilePath,
            string projectDir,
            string zipFilePath, 
            string unpackedDir)
        {
            // create project directory
            if (!Directory.Exists(projectDir))
            {
                try
                {
                    Directory.CreateDirectory(projectDir);

                    this.logger.Info("Created new directory for source project: " + projectDir);
                }
                catch (Exception ex)
                {
                    this.logger.Error("Exception when creating directory: " + projectDir + ". " , ex);
                    return false;
                }
            }
            else
            {
                this.logger.Error(projectDir + " already exists! Skipping this job.");
                return false;
            }

            // copy data from file server.
            bool rval = this.DownloadAndUnzip(job.ProjectSourceUri, zipFilePath, unpackedDir);
            if (!rval)
            {
                return false;
            }

            // check that envx file exists.
            if (!File.Exists(envxFilePath))
            {
                this.logger.Error("Envx project file " + envxFilePath + " does not exist, skipping job: " + job.Guid);
                return false;
            }
            return true;
        }

        // Download to the working directory and unpack.
        // 

        /// <summary>
        /// Downloads and unzip's job data.
        /// See setupJobData() for params description.
        /// </summary>
        /// <returns></returns>
        private bool DownloadAndUnzip(string projectSourceUri, string zipFilePath, string unpackedDir)
        {
            try
            {
                // download
                using (WebClient client = new WebClient())
                {
                    this.logger.Info("Starting download from: " + projectSourceUri);
                    Uri srcUri = new Uri(projectSourceUri);
                    Uri zipUri = new Uri(zipFilePath);
                    client.DownloadFile(srcUri, zipFilePath);
                    this.logger.Info("Done downloading.");
                }

                // make unpack-dir (the local working project dir)
                if (!Directory.Exists(unpackedDir))
                {
                    Directory.CreateDirectory(unpackedDir);
                    this.logger.Info("Created directory for unpacked project: " + unpackedDir);
                }

                // unzip project into dir.
                this.logger.Info("Unzipping file " + zipFilePath + " to " + unpackedDir);
                ZipFile.ExtractToDirectory(zipFilePath, unpackedDir);
                this.logger.Info("Done unzipping.");
            }
            catch (WebException e)
            {
                this.logger.Error("Web exception occurred, skipping job. ", e);
                return false;
            }
            catch (Exception e)
            {
                this.logger.Error("Exception when downloading and unpacking: ", e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Zip and upload the job results after a run.
        /// </summary>
        /// <param name="projectResultsUri"></param>
        /// <param name="jobGuid"></param>
        /// <param name="unpackedDir"></param>
        /// <param name="zipFileName"></param>
        /// <returns></returns>
        private bool ZipAndUpload(string projectResultsUri, string jobGuid, string unpackedDir, string zipFileName)
        {
            // create zip file.
            string zipname = jobGuid + ResultsAppendStr + ".zip";
            string zippath = Path.Combine(WorkingDir, zipname);
            try
            {
                this.logger.Info("Copying log files to project directory: " + unpackedDir);
                this.CopyEnvLogs(unpackedDir);
                this.logger.Info("Zipping " + zippath);
                ZipFile.CreateFromDirectory(unpackedDir, zippath);
            }
            catch (Exception e)
            {
                this.logger.Error("Failed zipping, skipping upload: ", e);
                return false;
            }

            // upload time.
            try
            {
                using (WebClient client = new WebClient())
                {
                    string z = Path.Combine(projectResultsUri, zipname);
                    Uri zipPath = new Uri(z);
                    this.logger.Info("Starting upload of finished job to " + z);
                    client.UploadFile(z, zippath);
                }
            } 
           catch (WebException e)
            {
                this.logger.Error("Trouble uploading the results zip file. ", e);
                return false;
            }
            catch (Exception e)
            {
                this.logger.Error(string.Empty, e);
                return false;
            }
            return true;
        }

        private void CopyEnvLogs(string unpackedDir)
        {
            try
            {
                string envDir = Path.GetDirectoryName(EnvExePath);
                string host = Environment.MachineName.ToLower();
                string rptlog = ConfigParser.Instance()["envReportLog"].ToLower();
                string lbklog = ConfigParser.Instance()["envLogbookLog"].ToLower();
                string envlog = ConfigParser.Instance()["envLog"].ToLower();
                string logdir = ConfigParser.Instance()["resultsLogDir"];
                foreach (string logfile in Directory.GetFiles(envDir, "*.log"))
                {
                    string log = Path.GetFileName(logfile).ToLower();
                    if (log.Contains(host))
                    {
                        if (log.StartsWith(rptlog))
                        {
                            rptlog = logfile;
                        }
                        else if (log.StartsWith(lbklog))
                        {
                            lbklog = logfile;
                        }
                        else if (log.StartsWith(envlog))
                        {
                            envlog = logfile;
                        }
                    }
                }

                string projLogDir = Path.Combine(unpackedDir, logdir);
                this.CheckForAndCreateDirectory(projLogDir);

                // copy original logs to project directory.
                this.CopySingleFile(rptlog, projLogDir, rptlog);
                this.CopySingleFile(lbklog, projLogDir, lbklog);
                this.CopySingleFile(envlog, projLogDir, envlog);

                // delete original logs
                this.DeleteSingleFile(rptlog);
                this.DeleteSingleFile(lbklog);
                this.DeleteSingleFile(envlog);

            }
            catch (Exception e)
            {
                string msg = string.Empty;
                if (e is NullReferenceException)
                {
                    msg =
                    "NullReferenceException when copying logs: " +
                    "Are all log paths and directories specified in the tracker configuration file?";
                }
                this.logger.Error("Exception in copyEnvLogs: " + msg, e);
            }
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
                    this.logger.Error("Could not create directory: " + dirPath);
                }
                rval = true;
            }
            return rval;
        }

        // 
        // 

        /// <summary>
        /// Copy single file, checks all params for invalid path and file characters.
        /// Catches all exceptions that might be thrown.
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
                    this.logger.Error("The file: " + sourceFile + " contains this invalid (filename) character: " +
                                   sourceFile.ElementAt(i));
                    throw new ArgumentException();
                }
                
                // Combine and GetFileName will check args for invalid chars.
                string path = Path.Combine(destDir, Path.GetFileName(destFile));

                File.Copy(sourceFile, destFile);
                this.logger.Info("Successfuly copied: " + sourceFile + " to " + destFile);
            } 
            catch (Exception e)
            {
                // string f = Path.GetFileName(sourceFile);
                this.logger.Error("Exception when copying file: " + sourceFile, e);
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
                this.logger.Error("Exception when deleting file: " + file, e);
            }
        }
    }
}
