package DataObjects;

import java.io.Serializable;
import java.util.HashMap;
import java.util.Map;

/**
 * @author jim
 * @class
 * @date 6/28/14
 */
public class TrackerMemoryInfo implements Serializable {
    private long totalMemory = Long.MIN_VALUE;
    private long availableMemory = Long.MIN_VALUE;
    private Map<String, Long> availableDiskSpace;

    public static TrackerMemoryInfo deepCopy(TrackerMemoryInfo rhs) {
        if (rhs == null) return null;

        TrackerMemoryInfo copy = new TrackerMemoryInfo();
        copy.totalMemory = rhs.totalMemory;
        copy.availableMemory = rhs.availableMemory;
        copy.availableDiskSpace = new HashMap<>(rhs.availableDiskSpace);

        return copy;
    }

    private TrackerMemoryInfo() { }

    public TrackerMemoryInfo(long totalMemory, long availableMemory) {
        this.totalMemory = totalMemory;
        this.availableMemory = availableMemory;
        availableDiskSpace = new HashMap<>();
    }

    public long getTotalMemory() {
        return totalMemory;
    }

    public void setTotalMemory(long totalMemory) {
        this.totalMemory = totalMemory;
    }

    public long getAvailableMemory() {
        return availableMemory;
    }

    public void setAvailableMemory(long availableMemory) {
        this.availableMemory = availableMemory;
    }

    public Map<String, Long> getAvailableDiskSpace() {
        return availableDiskSpace;
    }
}
