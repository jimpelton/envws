package Orchestrator;

import DataObjects.JobData;
import DataObjects.TrackerData;
import ServiceStubs.JobRequestServiceStub;
import org.apache.log4j.LogManager;
import org.apache.log4j.Logger;


/**
 * Service for supporting tracker operations such as job requests and checkin (ping).
 * @author jim
 * @class JobRequestService
 * @date 6/28/14
 */
public class JobRequestService implements JobRequestServiceStub {
    private static Logger logger = LogManager.getLogger(JobRequestService.class.getName());

    private JobsManager jobsManager;
    private TrackerManager trackerManager;

    public JobRequestService(JobsManager jobsManager, TrackerManager trackerManager) {
        this.jobsManager = jobsManager;
        this.trackerManager = trackerManager;
    }

    //////////////////////////////////////////////
    //   R M I   H A N D L E R S                //
    //////////////////////////////////////////////

    @Override
    public boolean ping(TrackerData td) {
        trackerManager.updateTracker(td);
        logger.trace("Got pinged: " + td.getHostName() + " (" + td.getUuid().toString()+")");

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
        logger.info("Handing out job: %s (%s).");

        return rval;
    }

    /**
     *
     * @param job The job to be returned.
     * @return true if job returned, false if no previous record of this job exists.
     */
    @Override
    public boolean returnJob(JobData job) {
        boolean rval;

        rval = jobsManager.returnJob(job);
        if (rval) {
            logger.info(String.format("Job %s (%s) returned from tracker %s (%s).", job.getFriendlyName(),
                    job.getUuid(), "", job.getTrackerUuid() ));
        } else {
            String msg = "Tracker %s tried to return JobData %s (%s), " +
                    "but that job was never actually submitted.";
            logger.error(String.format(msg, job.getTrackerUuid(), job.getFriendlyName(), job.getUuid()));
        }

        return rval;
    }

    @Override
    public void register(TrackerData td) {
        td.setLastCheckinTime(System.currentTimeMillis());
        trackerManager.addTracker(td);
    }
}
