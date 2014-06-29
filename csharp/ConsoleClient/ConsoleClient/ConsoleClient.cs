using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ConsoleClient
{
	using ClientLib.Client;
    using EnvwsLib.DataContracts;

    class ConsoleClient
    {

        private bool isInit = false;
        private OrchestratorConnection oc;
        private string orchestratorIpAddr;
        private ManualResetEvent manualResetEvent;
        private List<TrackerData> datas;
        private static object _datasMutex = new object();

        public ConsoleClient()
        {
            datas = new List<TrackerData>();
            manualResetEvent = new ManualResetEvent(false);
        }

        public bool Init()
        {
            if (!isInit)
            {
                oc = new OrchestratorConnection();
                oc.TrackersPinged += OnPingEvent;
                oc.StartPinging();
                isInit = true;
            }

            return isInit;
        }

        public void OnPingEvent(object sender, TrackersPingedEventArgs e)
        {
            datas.Clear();
            datas.AddRange(e.Trackers);
            manualResetEvent.Set();
        }

        
        public TrackerData[] GetTrackerData()
        {
            manualResetEvent.WaitOne();
            TrackerData[] rval;
            lock (_datasMutex)
            {
                rval = datas.ToArray();
            }

            return rval;
        }

        public void SubmitJob(string envxFileName, string sourceUri, string resultsUri, string friendlyName, int[] scenarios)
        {
			try
			{
                oc.SubmitJob(envxFileName, sourceUri, resultsUri, friendlyName, scenarios);
			}
			catch (System.Exception ex)
			{
                Console.WriteLine("Failed to submit job: " + ex.Message);					
			}
        }
    }
}
