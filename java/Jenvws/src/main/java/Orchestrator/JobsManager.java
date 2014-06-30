package Orchestrator;

import DataObjects.JobData;
import org.apache.log4j.LogManager;
import org.apache.log4j.Logger;

import java.util.*;

/**
 * @author jim
 * @class
 * @date 6/28/14
 */
public class JobsManager {
    private static final Logger logger = LogManager.getLogger(JobsManager.class.getName());

    private final Map<UUID, JobData> allJobs = new HashMap<>();

    private final List<JobData> waitingJobs = new ArrayList<>();

    private final Object _jobsListMutex = new Object();

    public void pushJob(JobData job) {

        synchronized (_jobsListMutex) {
            waitingJobs.add(job);
            allJobs.put(job.getUuid(), job);
        }

        logger.info("Put job " + job.getUuid() + " in the queue.");
    }

    /**
     *
     * @return
     */
    public JobData getJob() {

        JobData rval = JobData.emptyJob();

        synchronized (_jobsListMutex) {
            if (waitingJobs.size() > 0) {
                rval = waitingJobs.remove(0);
            }
        }

        return rval;
    }

    /**
     *
     * @param job
     * @return
     */
    public boolean returnJob(JobData job) {

        boolean rval = false;
        JobData actual;

        synchronized (_jobsListMutex) {
            actual = allJobs.replace(job.getUuid(), job);
        }

        if (actual != null) {
            rval = true;
        } else {
            String msg = "Tracker %s tried to return JobData %s (%s), " +
                    "but that job was never actually submitted.";
            logger.error(String.format(msg, job.getTrackerUuid(), job.getFriendlyName(), job.getUuid()));
        }

        return rval;
    }

    /**
     *
     * @return
     */
    public List<JobData> getAllJobs() {

        List<JobData> rval = new ArrayList<JobData>();

        synchronized (_jobsListMutex) {
            rval.addAll(allJobs.values());
        }

        return rval;
    }

}
