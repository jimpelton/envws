package Orchestrator;

import DataObjects.TrackerData;
import org.apache.log4j.Logger;
import org.apache.log4j.LogManager;

import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;

/**
 * @author jim
 * @class
 * @date 6/28/14
 */
public class TrackerManager {

    private static final Logger logger = LogManager.getLogger(TrackerManager.class.getName());

    private final ConcurrentHashMap<UUID, TrackerData> allTrackers = new ConcurrentHashMap<>();

    private int lateCheckInLimit;

    public static void scrubTrackerList(TrackerManager tm) {

    }

    public TrackerManager(int maxPingSpanMillis) {
        lateCheckInLimit = maxPingSpanMillis < 1000 ? 1000 : maxPingSpanMillis;
    }

    public void start() {

    }







}
