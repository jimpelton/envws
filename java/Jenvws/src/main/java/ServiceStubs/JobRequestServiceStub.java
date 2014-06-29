package ServiceStubs;

import DataObjects.JobData;
import DataObjects.TrackerData;

import java.rmi.Remote;

/**
 * @author jim
 * @class
 * @date 6/28/14
 */
public interface JobRequestServiceStub extends Remote {
    boolean ping(TrackerData tracker);
    JobData requestJob();
    boolean returnJob(JobData job);
}
