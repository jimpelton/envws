package DataObjects;

import java.io.Serializable;
import java.util.UUID;

/**
 * @author jim
 * @class
 * @date 6/27/14
 */
public class TrackerData implements Serializable {

    private UUID uuid;
    private String hostName = "";
    private TrackerStatus status = TrackerStatus.UNKNOWN;
    private long lastCheckinTime = Long.MIN_VALUE;
    private JobData currentJob;
    private TrackerMemoryInfo memoryInfo;

    public static TrackerData deepCopy(TrackerData rhs) {

        if (rhs == null) return rhs;

        TrackerData copy = new TrackerData();
        copy.uuid = UUID.fromString(rhs.uuid.toString());
        copy.hostName = rhs.hostName;
        copy.status = rhs.status;
        copy.lastCheckinTime = rhs.lastCheckinTime;
        copy.currentJob = (rhs.currentJob != null) ? JobData.deepCopy(rhs.currentJob) : JobData.emptyJob();
        copy.memoryInfo = TrackerMemoryInfo.deepCopy(rhs.memoryInfo);

        return copy;
    }

    private TrackerData() { }

    public TrackerData(UUID trackerId) {
        uuid = trackerId;
    }

    @Override
    public String toString() {

        String rval = "Guid: " + uuid + " Status: " + status;

        if (currentJob != null) {
            rval += " Current Job: " + currentJob.getEnvxName() + " (" + currentJob.getUuid() + ")";
        } else {
            rval += " Current Job: none";
        }

        return rval;
    }

    public UUID getUuid() {
        return uuid;
    }

    public void setUuid(UUID uuid) {
        this.uuid = uuid;
    }

    public String getHostName() {
        return hostName;
    }

    public void setHostName(String hostName) {
        this.hostName = hostName;
    }

    public TrackerStatus getStatus() {
        return status;
    }

    public void setStatus(TrackerStatus status) {
        this.status = status;
    }

    public long getLastCheckinTime() {
        return lastCheckinTime;
    }

    public void setLastCheckinTime(long lastCheckinTime) {
        this.lastCheckinTime = lastCheckinTime;
    }

    public JobData getCurrentJob() {
        return currentJob;
    }

    public void setCurrentJob(JobData currentJob) {
        this.currentJob = currentJob;
    }

    public TrackerMemoryInfo getMemoryInfo() {
        return memoryInfo;
    }

    public void setMemoryInfo(TrackerMemoryInfo memoryInfo) {
        this.memoryInfo = memoryInfo;
    }
}
