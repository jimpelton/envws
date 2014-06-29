package Orchestrator;

import DataObjects.JobData;
import DataObjects.TrackerData;
import ServiceStubs.JobRequestServiceStub;

/**
 * Service for supporting tracker operations such as job requests and checkin (ping).
 * @author jim
 * @class JobRequestService
 * @date 6/28/14
 */
public class JobRequestService implements JobRequestServiceStub{

    private JobsManager jobsManager;
    private TrackerManager trackerManager;

    public JobRequestService(JobsManager jobsManager, TrackerManager trackerManager) {
        this.jobsManager = jobsManager;
        this.trackerManager = trackerManager;
    }

    @Override
    public boolean ping(TrackerData tracker) {
        return true;
    }

    /**
     * Gets a job from the Orchestrator's JobManager. If there is not
     * waiting then JobData.emptyJob() is returned.
     * @return a JobData, or the Empty Job if no jobs are waiting.
     */
    @Override
    public JobData requestJob() {
        JobData rval;
        rval = jobsManager.getJob();
        return rval;
    }

    @Override
    public boolean returnJob(JobData job) {
        return false;
    }
}
