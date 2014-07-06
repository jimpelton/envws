package Tracker;

import DataObjects.JobData;
import Tasks.JobCompletedEventArgs;
import Tasks.JobScheduler;
import org.apache.log4j.LogManager;
import org.apache.log4j.Logger;

import java.util.concurrent.ArrayBlockingQueue;
import java.util.concurrent.Callable;

/**
 * @author jim
 * @class
 * @date 7/5/14
 */
public class JobRunner {
    private static Logger logger = LogManager.getLogger(JobRunner.class.getName());
    private static int MAX_WAITING = 1;
    private static int MAX_RUNNING = 1;


    /* Queue of Jobs to run */
    private ArrayBlockingQueue<JobData> jobRunnerQueue = new ArrayBlockingQueue<JobData>(MAX_WAITING);
    /* mutex on jobRunnerQueue */
    private final Object _jobRunnerQueueMutex = new Object();

    private Callable JobCompletedCallback;
    private Callable JobStartedCallback;

    private boolean keepWorking = false;
    private final Object _keepWorkingMutex = new Object();

    private int watchLoopJobIndex = -1;





    public void start() {
        boolean kw = getKeepWorking();

        if (!kw) {
            setKeepWorking(true);
            watchLoopJobIndex = JobScheduler.submitOneShot(this::watchLoop, 0);
        }
    }

    public void stop() {
        throw new UnsupportedOperationException("stop() not implemented yet.");
    }

    protected void onJobCompleted(JobCompletedEventArgs e) {

    }



    protected int watchLoop() throws InterruptedException {
        logger.info("Watch loop started.");
        while(getKeepWorking()) {
            logger.info("Tracker waiting for new Job");
            JobData job = jobRunnerQueue.take();
        }
        return 0;
    }

    public boolean addNewJob(JobData job) {
        return jobRunnerQueue.offer(job);
    }

    public int numJobsWaiting() {
        return jobRunnerQueue.size();
    }

    private boolean getKeepWorking() {
        boolean kw;
        synchronized (_keepWorkingMutex) {
            kw = keepWorking;
        }
        return kw;
    }

    private void setKeepWorking(boolean kw) {
        synchronized (_keepWorkingMutex) {
            keepWorking = kw;
        }
    }

}
