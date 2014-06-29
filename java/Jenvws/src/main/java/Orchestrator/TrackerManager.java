package Orchestrator;

import DataObjects.TrackerData;
import DataObjects.TrackerStatus;
import Tasks.JobScheduler;
import org.apache.log4j.Logger;
import org.apache.log4j.LogManager;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;

/**
 * @author jim
 * @class
 * @date 6/28/14
 */
public class TrackerManager {

    private static final Logger logger = LogManager.getLogger(TrackerManager.class.getName());

    private final List<TrackerData> allTrackers = Collections.synchronizedList(new ArrayList<TrackerData>());

    private int pingDelay;
    private int lateCheckInLimit;
    private int removeThresholdLimit;

    public void scrubTrackerList() {
        long now = System.currentTimeMillis();

        synchronized (allTrackers) {

            for (int i=0; i<allTrackers.size(); ++i) {

                TrackerData t = allTrackers.get(i);
                long delay = now - t.getLastCheckinTime();

                if (delay > lateCheckInLimit) {
                    if (delay > removeThresholdLimit)
                        allTrackers.remove(i);
                    else
                        t.setStatus(TrackerStatus.NO_RESPONSE);
                }
            } // for
        }
    }

    public TrackerManager(int maxPingSpanMillis, int lateCheckInLimitMillis, int removeLimitMillis) {

        this.pingDelay = (maxPingSpanMillis < 500) ? 500 : maxPingSpanMillis;
        this.lateCheckInLimit = lateCheckInLimitMillis;
        this.removeThresholdLimit = removeLimitMillis;

    }

    public TrackerManager(int maxPingSpanMillis) {
        this(maxPingSpanMillis, 3*maxPingSpanMillis, 6*maxPingSpanMillis);
    }

    public void startScrubLoop() {
        JobScheduler.submitRecurring(this::scrubTrackerList, pingDelay, pingDelay);

    }

    public void addTracker(TrackerData td) {
        td.setLastCheckinTime(System.currentTimeMillis());
        allTrackers.add(td);
    }
}
