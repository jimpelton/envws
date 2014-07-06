package Tracker;

import DataObjects.TrackerData;

/**
 * @author jim
 * @class
 * @date 7/5/14
 */
public abstract class JobRunner {

    


    public abstract int start(TrackerData td);
    protected abstract int run(TrackerJobData job);



}
