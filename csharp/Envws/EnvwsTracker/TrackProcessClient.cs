using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Threading;
using System.Linq;
using System.Collections.Generic;


namespace EnvwsTracker
{
    using log4net;
    using log4net.Config;
    
    using EnvwsLib.Util;
    using EnvwsLib.Events;
    using EnvwsLib.DataContracts;
    using EnvwsLib.ServiceProxies;
    using EnvwsLib.ServiceContracts;
    

	/// <summary>
	/// TrackProcessClient is a client to the Orchestrator service.
	/// </summary>
    public class TrackProcessClient
    {
        
        private static ILog logger;

        /// <summary>
        /// The current state of this tracker is kept in the TrackerData. By sending
        /// this TrackerData to the orchestrator, the orchestrator is kept up to date
        /// on what this tracker is currently up to.
        /// </summary>
        private TrackerData Data;

        /// <summary>
        /// This clients manager of job executions and status changes.
        /// </summary>
        private ProcessTrackerManager Manager { get; set; }

        /// <summary>
        /// Fires when the Orchestrator service should be pinged with update on 
        /// status, etc.
        /// </summary>
        private Timer PingTimer;

        /// <summary>
        /// Fires when the Orchestrator service should be queried for available job.
        /// </summary>
        private Timer CheckJobTimer;

        /// <summary>
        /// Interval for CheckJobTimer.
        /// </summary>
        private int RequestJobFreqMillis { get; set; }

        /// <summary>
        /// Interval for PingTimer.
        /// </summary>
        private int CheckInFreqMillis { get; set; }


        private const int CHECKIN_INTERVAL_NORMAL = 1000;
        
        private const int CHECKIN_INTERVAL_SLOW = 5000;

        /// <summary>
        /// A list of completed Jobs that this tracker has completed.
        /// </summary>
        private IList<TrackerJobData> CompletedJobs = new List<TrackerJobData>();

        /// <summary>
        /// Create a client with the given TrackerData.
        /// </summary>
        /// <param name="td">
        /// A tracker data already filled out with the information of this tracker.
        /// </param>
        public TrackProcessClient(TrackerData td /*, CheckInServiceClient client*/ )
        {
            Data = td;
            RequestJobFreqMillis = CHECKIN_INTERVAL_NORMAL;
            CheckInFreqMillis = CHECKIN_INTERVAL_NORMAL;
        }

        /// <summary>
        /// The client proxy for the Orchestrator check-in service. 
        /// 
        /// The name client is probably a poor choice, since this 
        /// TrackProcessClient object is the true client, and the 
        /// Orchestrator provides the service end-point.
        /// </summary>
        private CheckInServiceClientProxy Client { get; set; }
        
        /// <summary>
        /// Starts the ProcessTrackerManager for this process tracker. This method
        /// blocks until the orchestrator is automatically found (or a timeout occurs),
        /// then the ping loop and job request loops are started.
        /// This method requests a new job before it returns.
        /// </summary>
        /// <returns>
        /// False if the orchestrator was not found, true otherwise.
        /// </returns>
        public bool StartManager()
        {
            Client = InvokeCheckInService(FindCheckInService());
            if (Client != null)
            {
                PingTimer = new Timer(OnPingTimer, this, Timeout.Infinite, Timeout.Infinite);
                CheckJobTimer = new Timer(RequestNewJob, this, Timeout.Infinite, Timeout.Infinite);
                //this.StartPingLoop();
            }
            else
            {
				// An orchestrator was not found.
                return false;
            }

            Manager = new ProcessTrackerManager();

            Manager.JobCompleted += OnJobComplete;
            Manager.StatusChanged += OnStatusChanged;
            Manager.Start();

            RequestNewJob(this);
            return true;
        }

		/// <summary>
		/// Sends the CheckIn message and submits the current tracker data.
		/// </summary>
		/// <returns>
        /// False if the check in failed for some reason, true otherwise.
        /// </returns>
        public bool DoCheckIn()
        {
            bool rval = false;
            try
            {
                Data.Status = Manager.Status;
                Client.CheckIn(Data);
                rval = true;

                logger.Debug("Checked in.");
            }
            catch (EndpointNotFoundException e)
            {
                logger.Error("Orchestrator not found when checking in.", e);
            }

            return rval;
        }

		/// <summary>
		/// Handle an OnJobCompleteEvent, sent by the ProcessTrackerManager instance.
        /// Returns the finished jobdata object contained in <code>args</code>, then
        /// requests a new job.
		/// </summary>
        private void OnJobComplete(object sender, JobCompletedEventArgs args)
        {
			CompletedJobs.Add(
                new TrackerJobData() 
				{
                    TheJobInQuestion = args.Job,
                    HasBeenReturnedToTracker = false
                });

            ReturnFinishedJobs(args.Job);
            RequestNewJob(this);
        }

        /// <summary>
        /// Handle an OnStatusChangedEvent. 
        /// These events are sent by the ProcessTrackerManager when a status change occurs, such
        /// as a job starts, ends, fails, etc.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStatusChanged(object sender, StatusChangedEventArgs e)
        {
            Data.Status = e.Status;
        }

        /// <summary>
        /// Returns the given job. If any jobs are still waiting to be returned, then 
        /// those jobs are returned as well.
        /// </summary>
        /// 
        /// <returns>
        /// False if the Orchestrator's endpoint was not found, true otherwise.
        /// </returns>
        private bool ReturnFinishedJobs(JobData job)
        {
            bool rval = false;
            try
            {
				IEnumerable<TrackerJobData> unreturnedJobs = 
                    CompletedJobs.Where(j => !j.HasBeenReturnedToTracker);
				
				foreach (TrackerJobData j in unreturnedJobs)
				{
                    Client.ReturnFinishedJob(j.TheJobInQuestion);					
                }

                rval = true;
                RequestJobFreqMillis = CHECKIN_INTERVAL_NORMAL;
            }
            catch (EndpointNotFoundException e)
            {
                
                logger.Error("Orchestrator not found when returning new job.", e);
                RequestJobFreqMillis = CHECKIN_INTERVAL_SLOW;
            }

            return rval;
        }

		/// <summary>
		/// Requests a new job from the tracker.
		/// </summary>
        private void RequestNewJob(object state)
        {
            JobData jd;
            try
            {
                jd = Client.RequestJob();
                if (jd != null)
                {
                    logger.Info(
                        string.Format("Received new job from orchestrator: {0} ({1})", 
                            jd.FriendlyName, jd.Guid));

                    jd.TrackerGuid = Data.Guid;

					//TODO: check thread safety for AddNewJob.
                    Manager.AddNewJob(jd);
                    
                    logger.Info(
                        string.Format("Added new job to manager: {0} ({1})", 
                            jd.FriendlyName, jd.Guid));

                    // Stop job requests while working on current job.
                    RequestJobFreqMillis = Timeout.Infinite;
                }
                else
                {
                    // Orchestrator replied no jobs, set recheck time to one second.
                    RequestJobFreqMillis = CHECKIN_INTERVAL_NORMAL;
                    logger.Debug("No new jobs on orchestrator, checking back in one second.");
                }
            } 
            catch (EndpointNotFoundException e)
            {
                logger.Error("Orchestrator not found when requesting new job.", e);

                // If orchestrator is not found, back off the job request times.
                RequestJobFreqMillis = CHECKIN_INTERVAL_SLOW;
            }
            finally
            {
                // Restart the timer.
                CheckJobTimer.Change(RequestJobFreqMillis, Timeout.Infinite);
            }
        }

        private void OnPingTimer(object state)
        {
            DoCheckIn();
            PingTimer.Change(CheckInFreqMillis, Timeout.Infinite);
        }

        public void StartPingLoop()
        {
            PingTimer.Change(CheckInFreqMillis, Timeout.Infinite);
            logger.Info("Starting ping loop.");
        }

		/// <summary>
		/// Searches for the orchestrator checkin service.
		/// </summary>
		/// <returns>The EndpointAddress of the first checkinservice found, otherwise null.</returns>
        private EndpointAddress FindCheckInService()
        {
            DiscoveryClient discoveryClient = 
                new DiscoveryClient(new UdpDiscoveryEndpoint());
            
            FindResponse findResponse = 
                discoveryClient.Find(new FindCriteria(typeof(ICheckInService)));

            EndpointAddress rval = null;

            if (findResponse.Endpoints.Count > 0)
            {
                rval = findResponse.Endpoints[0].Address;
            }

            return rval;
        }

		/// <summary>
		/// Connect to the service at <code>endpointAddress</code>.
		/// </summary>
        private CheckInServiceClientProxy InvokeCheckInService(EndpointAddress endpointAddress)
        {
            if (endpointAddress == null)
            {
                return null;
            }

            CheckInServiceClientProxy client 
                = new CheckInServiceClientProxy(new BasicHttpBinding(), endpointAddress);

            return client;
        }



        
    } // TrackProcessClient
}
