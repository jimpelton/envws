using System;
using System.Runtime.Serialization;

namespace EnvwsLib.DataContracts
{
    public enum JobStatus
    {
        QUEUED,           // Queued on orchestrator
        RUNNNG,           // Running on tracker
        COMPLETE,         // Completed

        FAILED           // check the logs (always, the logs!) !
        
    }

    [DataContract]
    public class JobData
    {
        /// <summary>
        /// Gets or sets a Guid for this job.
        /// </summary>
        [DataMember]
        public string Guid { get; set; }

        /// <summary>
        /// Gets or sets the name of the .envx project file for this Job.
        /// </summary>
        [DataMember]
        public string EnvxName { get; set; }

        /// <summary>
        /// Gets or sets the uri where the project file(s) can be downloaded from.
        /// </summary>
        [DataMember]
        public string ProjectSourceUri { get; set; }

        /// <summary>
        /// Uri where project results should be uploaded.
        /// </summary>
//        [DataMember]
//        public string ProjectResultsUri { get; set; }

        /// <summary>
        /// Gets or sets an int array of scenarios that will be run.
        /// </summary>
        [DataMember]
        public int[] ProjectScenarios { get; set; }

        /// <summary>
        /// Gets or sets a long that is the submit time of the job.
        /// </summary>
        [DataMember]
        public long SubmitTime { get; set; }

        /// <summary>
        /// Gets or sets a long that is the time this job began executing.
        /// </summary>
        [DataMember]
        public long StartTime { get; set; }

        /// <summary>
        /// Gets or sets an int array of scenarios that will be run.
        /// </summary>
        [DataMember]
        public long FinishTime { get; set; }

        /// <summary>
        /// Gets or sets a name submitted by the client to identify the job easier 
        /// (instead of using the guid).
        /// </summary>
        [DataMember]
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the exit code of the envision process.
        /// Initialized to -1.
        /// </summary>
        [DataMember]
        public int EnvisionExitCode { get; set; }

        /// <summary>
        /// Gets or sets the tracker's Guid as a string.
        /// </summary>
        [DataMember]
        public string TrackerGuid { get; set; }

        [DataMember]
        public JobStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the private working set size (memory usage) of this job.
        /// </summary>
        //TODO: JobPrivateWorkingSetSize not used.
        [DataMember]
        public long JobPrivateWorkingSetSize { get; set; }

        public static JobData EmptyJob { get { return _emptyJob; } }
        private static readonly JobData _emptyJob = new JobData("EMPTY_JOB");

        private JobData(string guid)
        {
            Guid = guid;
            EnvxName = "";
            ProjectSourceUri = "";
            ProjectScenarios = new int[0];
            SubmitTime = 0L;
            FinishTime = 0L;
            StartTime = 0L;
            FriendlyName = "";
            EnvisionExitCode = -1;
            TrackerGuid = "";
            JobPrivateWorkingSetSize = -1L;
        }

        public JobData() : this("") { }


        public static JobData DeepCopy(JobData rhs)
        {
            if (rhs == null) return null;

            JobData copy = new JobData()
            {
                Guid = string.Copy(rhs.Guid),
                EnvxName = string.Copy(rhs.EnvxName),
                ProjectSourceUri = string.Copy(rhs.ProjectSourceUri),
                SubmitTime = rhs.SubmitTime,
                FinishTime = rhs.FinishTime,
                StartTime = rhs.StartTime,
                FriendlyName = string.Copy(rhs.FriendlyName),
                EnvisionExitCode = rhs.EnvisionExitCode,
                TrackerGuid = string.Copy(rhs.TrackerGuid),
                JobPrivateWorkingSetSize = rhs.JobPrivateWorkingSetSize
            };

            copy.ProjectScenarios = new int[rhs.ProjectScenarios.Length];
            Array.Copy(rhs.ProjectScenarios, copy.ProjectScenarios, copy.ProjectScenarios.Length);
            
            return copy;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            JobData j = obj as JobData;
            return Guid.Equals(j.Guid);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
