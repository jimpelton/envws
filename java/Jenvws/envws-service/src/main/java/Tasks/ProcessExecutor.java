package Tasks;

import DataObjects.JobData;

import java.util.concurrent.Callable;

/**
 *
 */
public abstract class ProcessExecutor<ReturnType> {

    /* Currently running job */
    private JobData currentJob;

    private Callable ProcessCompletedCallback;

    ProcessExecutor(JobData job, Callable processCompletedCallback){
        currentJob = job;
        this.ProcessCompletedCallback = processCompletedCallback;
    }

    public abstract ReturnType executeJob();

    public JobData getCurrentJob() {
        return currentJob;
    }

    protected void setCurrentJob(JobData job) {
        currentJob = job;
    }

    public void setProcessCompletedCallback(Callable c) {
        ProcessCompletedCallback = c;
    }

    protected Callable getProcessCompletedCallback() {
        return ProcessCompletedCallback;
    }
}
