package ServiceStubs;

import DataObjects.JobData;
import DataObjects.TrackerData;

import java.rmi.Remote;
import java.rmi.RemoteException;

/**
 * @author jim
 * @class
 * @date 6/28/14
 */
public interface JobRequestServiceStub extends Remote {
    boolean ping(TrackerData tracker) throws RemoteException;
    JobData requestJob() throws RemoteException;
    boolean returnJob(JobData job) throws RemoteException;
}
