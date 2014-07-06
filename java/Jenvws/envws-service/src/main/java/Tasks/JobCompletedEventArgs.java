package Tasks;

import DataObjects.JobData;

/**
 *
 */
public class JobCompletedEventArgs {

    private JobData jd;
    private int exitCode;

    public JobCompletedEventArgs(JobData jd, int exitCode) {
        this.jd = jd;
        this.exitCode = exitCode;
    }

    public JobData getJd() {
        return jd;
    }

    public int getExitCode() {
        return exitCode;
    }

    //    public void setJd(JobData jd) {
//        this.jd = jd;
//    }
}
