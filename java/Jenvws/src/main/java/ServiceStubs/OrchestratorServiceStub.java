package ServiceStubs;

import DataObjects.JobData;
import DataObjects.TrackerData;

import java.rmi.Remote;

/**
 * @author jim
 * @class
 * @date 6/27/14
 */
public interface OrchestratorServiceStub extends Remote {
    //boolean ping(TrackerData data);
    void submitJob(JobData job);
    JobData[] getAllJobs();
    TrackerData[] getAllTrackers();

}
