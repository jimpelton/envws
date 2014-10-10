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
        public string HostName { get; set; }

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
        /// If there is no Job running, CurrentJob should be set to the EmptyJob.
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
        /// 
        /// Default values of the TrackerData:
        /// The Guid and HostName are both initialized to string.Empty.
        /// The Status is initialized to TrackerStatus.IDLE.
        /// LastCheckinTime is initialized to -1.
        /// CurrentJob is initialized to JobData.EmptyJob.
        /// MemoryInfo is set to a new TrackerMemoryInfo() instance.
        /// </summary>
        public TrackerData()
        {
            Guid = string.Empty;
            HostName = string.Empty;
            Status = TrackerStatus.IDLE;
            LastCheckinTime = -1;
            CurrentJob = JobData.EmptyJob;
            MemoryInfo = new TrackerMemoryInfo();
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
                HostName = string.Copy(rhs.HostName),
                Status = rhs.Status,
                LastCheckinTime = rhs.LastCheckinTime,
                CurrentJob = JobData.DeepCopy(rhs.CurrentJob) ?? JobData.EmptyJob
            };

            return copy;
        }

        protected bool Equals(TrackerData other)
        {
            return string.Equals(Guid, other.Guid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TrackerData) obj);
        }

        public override int GetHashCode()
        {
            return (Guid != null ? Guid.GetHashCode() : 0);
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
