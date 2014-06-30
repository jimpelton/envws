package Orchestrator;

import DataObjects.TrackerData;
import DataObjects.TrackerStatus;
import Tasks.JobScheduler;
import org.apache.log4j.Logger;
import org.apache.log4j.LogManager;

import java.util.*;
import java.util.concurrent.ConcurrentHashMap;

/**
 * @author jim
 * @class
 * @date 6/28/14
 */
public class TrackerManager {

    private static final Logger logger = LogManager.getLogger(TrackerManager.class.getName());

    private final List<TrackerData> allTrackers = Collections.synchronizedList(new ArrayList<TrackerData>());

    /* Delay between list scrubs. */
    private final int scrubDelay;

    /* Late tracker time limit */
    private int lateCheckInLimit;

    /* Really late, removed tracker time limit */
    private int removeThresholdLimit;

    /**
     *  Scrubs the tracker list marking late trackers, and removing the
     *  really late trackers.
     */
    private void scrubTrackerList() {

        long now = System.currentTimeMillis();

        synchronized (allTrackers) {

            for (int i=0; i<allTrackers.size(); ++i) {
                TrackerData t = allTrackers.get(i);
                long delay = now - t.getLastCheckinTime();

                if (delay > lateCheckInLimit) {
                    if (delay > removeThresholdLimit) {
                        allTrackers.remove(i);

                        String msg = String.format("Removed non-responsive tracker: %s (%s).", t.getHostName(), t.getUuid().toString());
                        logger.info(msg);
                    } else {
                        t.setStatus(TrackerStatus.NO_RESPONSE);

                        String msg = String.format("Set NO_RESPONSE status for: %s (%s).", t.getHostName(), t.getUuid().toString());
                        logger.info(msg);
                    }
                }
            } // for
        }
    }

    public TrackerManager(int scrubDelay, int lateCheckInLimitMillis, int removeLimitMillis) {
        this.scrubDelay = scrubDelay;
        this.lateCheckInLimit = lateCheckInLimitMillis;
        this.removeThresholdLimit = removeLimitMillis;
    }

    /**
     *
     */
    public void startScrubLoop() {
        JobScheduler.submitRecurring(this::scrubTrackerList, scrubDelay, scrubDelay);
    }

    /**
     *
     * @param td
     */
    public void addTracker(TrackerData td) {

        td.setLastCheckinTime(System.currentTimeMillis());

        synchronized (allTrackers) {
            allTrackers.add(td);
        }

        logger.info(String.format("Tracker added: %s (%s).", td.getHostName(), td.getUuid().toString()));
    }

    /**
     *
     * @param td
     */
    public void updateTracker(TrackerData td) {

        td.setLastCheckinTime(System.currentTimeMillis());

        synchronized (allTrackers) {
            allTrackers.replaceAll(cur -> (cur.getUuid().equals(td.getUuid())) ? td : cur);
        }

        logger.trace(String.format("Tracker updated: %s (%s).", td.getHostName(), td.getUuid().toString()));
    }

    /**
     * Finds a tracker with status IDLE, sets that tracker's status to TRANSITION and returns it.
     * @return null if there are no free trackers, otherwise a tracker marked with TRANSITION status.
     */
    public TrackerData getFreeTracker() {

        TrackerData td = null;

        synchronized (allTrackers) {

            Iterator<TrackerData> it = allTrackers.iterator();

            while (it.hasNext()) {
                td = it.next();
                if (td.getStatus() == TrackerStatus.IDLE) {
                    td.setStatus(TrackerStatus.TRANSITION);
                    break;
                }
            }
        }

        return td;
    }

    public void returnTracker(TrackerData td) {
        td.setStatus(TrackerStatus.IDLE);
        updateTracker(td);
    }
}
