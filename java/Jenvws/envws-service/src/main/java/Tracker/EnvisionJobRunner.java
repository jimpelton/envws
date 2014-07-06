package Tracker;

import DataObjects.JobData;

/**
 * @author jim
 * @class
 * @date 7/5/14
 */
public class EnvisionJobRunner implements JobRunner {

    /* Currently running job */
    private JobData currentJob = JobData.emptyJob();



    @Override
    public int run(TrackerJobData job) {
        return 0;
    }
}
