using System;
using System.Collections.Generic;
using EnvwsLib.DataContracts;

namespace EnvwsLib.Client
{
    public class TrackersPingedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the IEnumerable of trackers that are currently known.
        /// </summary>
        public IEnumerable<TrackerData> Trackers
        {
            get
            {
                return _trackers;
            }
        } 
        private IEnumerable<TrackerData> _trackers;

        public TrackersPingedEventArgs(IEnumerable<TrackerData> trackers)
        {
            _trackers = trackers;
        }
    }
}