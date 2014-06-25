using System;

namespace EnvwsTracker
{
    using EnvwsLib;
    using EnvwsLib.DataContracts;

    public class JobCompletedEventArgs : EventArgs
    {
        public JobData Job
        {
            get
            {
                return _data;
            }
            set
            {
                _data = JobData.DeepCopy(value);
            }
        }

        private JobData _data = JobData.EmptyJob;
    }
}