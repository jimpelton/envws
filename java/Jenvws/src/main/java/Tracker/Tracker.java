package Tracker;

import java.util.concurrent.ScheduledThreadPoolExecutor;

/**
 * @author jim
 * @class
 * @date 6/27/14
 */
public class Tracker {

    private final int MAX_THREAD = 10;

    private ScheduledThreadPoolExecutor executor
            = new ScheduledThreadPoolExecutor(MAX_THREAD);

    public Tracker(){ }

}
