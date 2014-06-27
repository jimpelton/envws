using EnvwsLib.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvwsTracker 
{
	/// <summary>
	/// Associates a completed JobData with a boolean that indicates if the JobData
    /// was successfully returned by the Tracker that executed it.
	/// </summary>
    public class TrackerJobData
    {
        public bool HasBeenReturnedToTracker { get; set; }
        public JobData TheJobInQuestion { get; set; }

		public TrackerJobData()
        {
            HasBeenReturnedToTracker = false;
        }
    }
}
