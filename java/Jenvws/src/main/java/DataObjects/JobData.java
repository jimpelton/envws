package DataObjects;

import java.io.Serializable;
import java.util.Arrays;
import java.util.UUID;

/**
 * @author jim
 * @class
 * @date 6/27/14
 */
public class JobData implements Serializable {
        /**
         * Gets or sets a Guid for this job.
         */
        private UUID uuid;

        /// <summary>
        /// Gets or sets the name of the .envx project file for this Job.
        /// </summary>
        private String envxName = "UNKNOWN";

        /// <summary>
        /// Gets or sets the uri where the project file(s) can be downloaded from.
        /// </summary>
        private String projectSourceUri = "UNKNOWN";

        /// <summary>
        /// Uri where project results should be uploaded.
        /// </summary>
        private String projectResultsUri = "UNKNOWN";

        /// <summary>
        /// Gets or sets an int array of scenarios that will be run.
        /// </summary>
         
        private int[] projectScenariosArray;

        /// <summary>
        /// Gets or sets a long that is the submit time of the job.
        /// </summary>
         
        private long SubmitTime = Long.MIN_VALUE;

        /// <summary>
        /// Gets or sets a long that is the time this job began executing.
        /// </summary>
         
        public long StartTime = Long.MIN_VALUE;

        /// <summary>
        /// Gets or sets an int array of scenarios that will be run.
        /// </summary>
         
        public long FinishTime = Long.MIN_VALUE;

        /// <summary>
        /// Gets or sets a name submitted by the client to identify the job easier 
        /// (instead of using the guid).
        /// </summary>
         
        public String FriendlyName = "UNKNOWN";

        /// <summary>
        /// Gets or sets the exit code of the envision process.
        /// Initialized to -1.
        /// </summary>
         
        public int EnvisionExitCode = Integer.MIN_VALUE;

        /// <summary>
        /// Gets or sets the tracker's Guid as a String.
        /// </summary>
         
        public UUID TrackerUuid;

        /// <summary>
        /// Gets or sets the private working set size (memory usage) of this job.
        /// </summary>
         
        public long JobPrivateWorkingSetSize = Long.MIN_VALUE;

        public static JobData emptyJob() {
            return _emptyJob;
        }

        private static final JobData _emptyJob = new JobData();

        private JobData() { }

        public JobData(UUID jobId) {
            this.uuid = jobId;
        }


        public static JobData deepCopy(JobData rhs) {

            if (rhs == null) return null;

            JobData copy = new JobData();
            copy.uuid = UUID.fromString(rhs.uuid.toString());
            copy.envxName = rhs.envxName;
            copy.projectSourceUri = rhs.projectSourceUri;
            copy.projectResultsUri = rhs.projectResultsUri;
            copy.projectScenariosArray = Arrays.copyOf(rhs.projectScenariosArray, rhs.projectScenariosArray.length);
            copy.SubmitTime = rhs.SubmitTime;
            copy.FinishTime = rhs.FinishTime;
            copy.FriendlyName = rhs.FriendlyName;
            copy.EnvisionExitCode = rhs.EnvisionExitCode;
            copy.TrackerUuid = UUID.fromString(rhs.uuid.toString());
            copy.JobPrivateWorkingSetSize = rhs.JobPrivateWorkingSetSize;

            return copy;
        }

    public UUID getUuid() {
        return uuid;
    }

    public void setUuid(UUID uuid) {
        this.uuid = uuid;
    }

    public String getEnvxName() {
        return envxName;
    }

    public void setEnvxName(String envxName) {
        this.envxName = envxName;
    }

    public String getProjectSourceUri() {
        return projectSourceUri;
    }

    public void setProjectSourceUri(String projectSourceUri) {
        this.projectSourceUri = projectSourceUri;
    }

    public String getProjectResultsUri() {
        return projectResultsUri;
    }

    public void setProjectResultsUri(String projectResultsUri) {
        this.projectResultsUri = projectResultsUri;
    }

    public int[] getProjectScenariosArray() {
        return projectScenariosArray;
    }

    public void setProjectScenariosArray(int[] projectScenariosArray) {
        this.projectScenariosArray = projectScenariosArray;
    }

    public long getSubmitTime() {
        return SubmitTime;
    }

    public void setSubmitTime(long submitTime) {
        SubmitTime = submitTime;
    }

    public long getStartTime() {
        return StartTime;
    }

    public void setStartTime(long startTime) {
        StartTime = startTime;
    }

    public long getFinishTime() {
        return FinishTime;
    }

    public void setFinishTime(long finishTime) {
        FinishTime = finishTime;
    }

    public String getFriendlyName() {
        return FriendlyName;
    }

    public void setFriendlyName(String friendlyName) {
        FriendlyName = friendlyName;
    }

    public int getEnvisionExitCode() {
        return EnvisionExitCode;
    }

    public void setEnvisionExitCode(int envisionExitCode) {
        EnvisionExitCode = envisionExitCode;
    }

    public UUID getTrackerUuid() {
        return TrackerUuid;
    }

    public void setTrackerUuid(UUID trackerUuid) {
        TrackerUuid = trackerUuid;
    }

    public long getJobPrivateWorkingSetSize() {
        return JobPrivateWorkingSetSize;
    }

    public void setJobPrivateWorkingSetSize(long jobPrivateWorkingSetSize) {
        JobPrivateWorkingSetSize = jobPrivateWorkingSetSize;
    }

    @Override
    public boolean equals(Object o) {

        if (this == o) return true;

        if (o == null || getClass() != o.getClass()) return false;

        JobData jobData = (JobData) o;

        if (!uuid.equals(jobData.uuid)) return false;

        return true;
    }

    @Override
    public int hashCode() {
        return getUuid().hashCode();
    }


}
