
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using EnvwsLib.DataContracts;

namespace TrackProcess
{
    public class StatusChangedEventArgs : EventArgs 
    {
        public TrackerStatus Status { get; set; }

        public StatusChangedEventArgs() { }
    }
}
