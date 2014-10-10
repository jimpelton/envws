using System;
using EnvwsLib.DataContracts;

namespace EnvwsLib.Events
{
    public class StatusChangedEventArgs : EventArgs 
    {
        public TrackerStatus Status { get; set; }

        public StatusChangedEventArgs() { }
    }
}
