package Tasks;

import DataObjects.JobData;

import java.util.concurrent.Callable;

/**
 * @author jim
 * @class
 * @date 7/5/14
 */
public class EnvisionJobExecutor extends ProcessExecutor<Integer> {

    private String envExePath;

    private String workingDir;

    private String resultsAppendString;

    public EnvisionJobExecutor(JobData job, Callable completedCallback) {
        super(job, completedCallback);
    }


    @Override
    public Integer executeJob() {
        return null;
    }
}
