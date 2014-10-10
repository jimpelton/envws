using System;
using System.Collections.Generic;

namespace EnvwsLib.Client
{
    public class SubmitJobEventArgs : EventArgs
    {
        public List<int> Scenarios { get; set; }

        public List<string> EnvxFiles { get; set; }

        public string ProjectSourceUri { get; set; }

        public string ProjectResultsUri { get; set; }

        public string FriendlyName { get; set; }

        public SubmitJobEventArgs()
        {
        }
    }
}
