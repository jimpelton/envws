using System.ServiceModel;

namespace EnvwsLib.ServiceContracts
{
    using EnvwsLib.DataContracts;
    using EnvwsLib.Tracker;

    /// <summary>
    /// Defines the Orchestrator service, which is exposed as the outside
    /// facing end user service that a web service should consume. 
    /// Exposes the interface for queing new jobs and getting tracker statuses.
    /// </summary>
    [ServiceContract(Name="EvnwsOrchestrator",
        Namespace = "http://EnvwsOrchestrator")]
    public interface IOrchestratorService
    {
        [OperationContract]
        bool QueueJob(JobData job);

        [OperationContract]
        bool RemoveJob(string jobGuid);

        [OperationContract]
        TrackerData[] TrackerStatus();
        
        [OperationContract]
        bool Ping();

        [OperationContract]
        int NumWaitingJobs();

        [OperationContract]
        int NumRunningTrackers();
        
        [OperationContract]
        int NumIdleTrackers();
    }
}