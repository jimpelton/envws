package ServiceStubs;

import DataObjects.JobData;
import DataObjects.TrackerData;

import java.rmi.Remote;
import java.rmi.RemoteException;

/**
 * @author jim
 * @class
 * @date 6/27/14
 */
public interface OrchestratorServiceStub extends Remote {
    void submitJob(JobData job) throws RemoteException;
    JobData[] getAllJobs() throws RemoteException;
    TrackerData[] getAllTrackers() throws RemoteException;
}
