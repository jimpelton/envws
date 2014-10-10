namespace EnvwsLib.ServiceContracts
{
    using System.ServiceModel;

    using EnvwsLib;
    using EnvwsLib.DataContracts;
    using EnvwsLib.Tracker;

    [ServiceContract(Name="EnvwsCheckinService",
        Namespace = "http://EnvwsCheckin")]
    public interface ICheckInService
    {
        // Allow Trackers to submit status updates
        [OperationContract]
        bool CheckIn(TrackerData td);
        
        // Trackers request new Job descriptions.
        [OperationContract]
        JobData RequestJob();
        
        // Trackers return job descriptions when done.
        [OperationContract]
        void ReturnFinishedJob(JobData j);
    }
}