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
        [OperationContract]
        bool CheckIn(TrackerData td);

        [OperationContract]
        JobData RequestJob();
        
        [OperationContract]
        void ReturnFinishedJob(JobData j);
    }
}