using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EnvwsLib.Tracker
{
    public class TrackerMemoryInfo
    {
        [DataMember]
        public long TotalMemory { get; set; }

        [DataMember]
        public long AvailableMemory { get; set; }

        [DataMember]
        public long TotalDiskSpace { get; set; }

        [DataMember]
        public Dictionary<string, long> AvailableDiskSpace { get; set; }

        public TrackerMemoryInfo()
        {
            TotalMemory = -1L;
            AvailableMemory = -1L;
            TotalDiskSpace = -1L;
            AvailableDiskSpace = new Dictionary<string, long>();
        }

        public static TrackerMemoryInfo DeepCopy(TrackerMemoryInfo rhs)
        {
            if (rhs == null)
            {
                return null;
            }

            TrackerMemoryInfo copy = new TrackerMemoryInfo()
            {
                TotalMemory = rhs.TotalMemory,
                AvailableMemory = rhs.AvailableMemory,
                TotalDiskSpace = rhs.TotalDiskSpace,
                AvailableDiskSpace = new Dictionary<string, long>(rhs.AvailableDiskSpace)
            };
         
            return copy;
        }
    }
}
