using System.Runtime.Serialization;

namespace EnvwsLib.DataContracts
{
	using EnvwsLib.Tracker;

    public enum TrackerStatus
    {
        RUNNING,
        IDLE,
        NO_RESPONSE,
        UNKNOWN
    }
    
    [DataContract]
    public class TrackerData
    {
        /// <summary>
        /// Gets or sets the unique identifier for the tracker
        /// represented by this TrackerData.
        /// </summary>
        [DataMember]
        public string Guid { get; set; }

        /// <summary>
        /// Gets or sets a string representing the Uri of the 
        /// tracker for this TrackerData.
        /// </summary>
        [DataMember]
        public string Uri { get; set; }

        /// <summary>
        /// Gets or sets the TrackerStatus of the tracker represented by
        /// this TrackerData.
        /// </summary>
        [DataMember]
        public TrackerStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a long representing the millies since 1970.
        /// </summary>
        [DataMember]
        public long LastCheckinTime { get; set; }

        /// <summary>
        /// Gets or sets the current job this tracker is working on.
        /// </summary>
        [DataMember]
        public JobData CurrentJob { get; set; }

        /// <summary>
        /// Gets or sets the TrackerMemoryInfo for this tracker.
        /// </summary>
        [DataMember]
        public TrackerMemoryInfo MemoryInfo { get; set; }

        
        /// <summary>
        /// Initializes a new instance of the <see cref="TrackerData"/> class. 
        /// </summary>
        public TrackerData()
        {
            Guid = string.Empty;
            Uri = string.Empty;
            Status = TrackerStatus.IDLE;
            LastCheckinTime = -1;
            CurrentJob = JobData.EmptyJob;
        }
        
        /// <summary>
        /// Make a deep copy of <code>rhs</code>.
        /// If rhs is null, then null is returned.
        /// </summary>
        /// <param name="rhs">
        /// The <see cref="TrackerData"/> to make a copy of.
        /// </param>
        /// <returns>
        /// A TrackerData that is a copy of <code>rhs</code>, null if <code>rhs</code> is null.
        /// </returns>
        public static TrackerData DeepCopy(TrackerData rhs)
        {
            if (rhs == null)
            {
                return null;
            }

            TrackerData copy = new TrackerData()
            {
                Guid = string.Copy(rhs.Guid),
                Uri = string.Copy(rhs.Uri),
                Status = rhs.Status,
                LastCheckinTime = rhs.LastCheckinTime,
                CurrentJob = JobData.DeepCopy(rhs.CurrentJob) ?? JobData.EmptyJob
            };

            return copy;
        }

        public override string ToString()
        {
            string rval = "Guid: " + Guid + " Status: " + Status;
            if (null != CurrentJob)
            {
                rval += " Current Job: " + CurrentJob.EnvxName + " (" + CurrentJob.Guid + ")";
            }
            else
            {
                rval += " Current Job: none";
            }

            return rval;
        }

    }
}
