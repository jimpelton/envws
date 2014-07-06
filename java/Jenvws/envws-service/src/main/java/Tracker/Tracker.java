package Tracker;


import DataObjects.JobData;
import DataObjects.TrackerData;
import DataObjects.TrackerStatus;
import ServiceStubs.JobRequestServiceStub;
import Tasks.JobScheduler;
import org.apache.log4j.LogManager;
import org.apache.log4j.Logger;

import java.net.InetAddress;
import java.rmi.NotBoundException;
import java.rmi.RemoteException;
import java.rmi.registry.LocateRegistry;
import java.rmi.registry.Registry;
import java.util.ArrayList;
import java.util.List;
import java.util.Queue;
import java.util.UUID;

/**
 * @author jim
 * @class
 * @date 6/27/14
 */
public class Tracker {
    private static Logger logger = LogManager.getLogger(Tracker.class.getName());

    /* Queue of Jobs to run */
    private List<JobRunner> jobRunnerQueue = new ArrayList<>();

    /* Max jobs in queue at a time. */
    private static int MAX_JOB_QUEUE_LENGTH = 1;

    private InetAddress orchAddr;
    private int orchPort;

    /* info about this tracker */
    private TrackerData trackerData;

    /* the orchestrator for this cluster */
    private JobRequestServiceStub orchestrator;

    /* the JobScheduler's index of the job running the ping loop */
    private int pingLoopJobIndex;

    public Tracker(String hostname, InetAddress orchAddr, int orchPort) {
        this.orchAddr = orchAddr;
        this.orchPort = orchPort;
        trackerData = new TrackerData(UUID.randomUUID());
        trackerData.setStatus(TrackerStatus.IDLE);
        trackerData.setHostName(hostname);
    }

    public void findRegistryAndCreateStub() throws RemoteException, NotBoundException {
        Registry registry = LocateRegistry.getRegistry(orchAddr.getHostName(), orchPort);
        orchestrator = (JobRequestServiceStub) registry.lookup("JobRequestService");
    }

    public void startPingLoop() {
        pingLoopJobIndex = JobScheduler.Instance().submitRecurring(this::ping, 1000, 1000);
    }

    /* ping the orchestrator, called by job submitted to JobScheduler */
    private void ping() {
        logger.trace("");
        try {
            boolean hasJobs = orchestrator.ping(trackerData);

            if (hasJobs) {
                JobData jd = orchestrator.requestJob();
                if (!jd.equals(JobData.emptyJob())) {

                }
            }
            logger.trace("Pinged orchcestrator");
        } catch (RemoteException e) {
            logger.error(e);
        }



    }

    public void setTrackerStatus(TrackerStatus ts) {
        trackerData.setStatus(ts);
    }

    public TrackerStatus getTrackerStatus() {
        return trackerData.getStatus();
    }

//    /**
//     * Request a new job from the orchestrator.
//     * @return Returns null if no job is on the orchestrator.
//     * @throws RemoteException
//     */
//    public JobData requestJob() throws RemoteException {
//        return orchestrator.requestJob();
//    }
//
//    /**
//     *
//     * @param job
//     * @return
//     * @throws RemoteException
//     */
//    public boolean returnJob(JobData job) throws RemoteException {
//        return false;
//    }
}
