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
import java.util.UUID;

/**
 * @author jim
 * @class
 * @date 6/27/14
 */
public class Tracker {
    private static Logger logger = LogManager.getLogger(Tracker.class.getName());

    private final int MAX_THREAD = 10;

    private InetAddress orchAddr;
    private int orchPort;
    private TrackerData trackerData;
    private JobRequestServiceStub orchestrator;

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

    /* ping the orchestrator */
    private void ping() {
        logger.trace("");
        try {
            orchestrator.ping(this.trackerData);
            logger.trace("Pinged orchcestrator");
        } catch (RemoteException e) {
            logger.error(e);
        }

    }

    /**
     * Request a new job from the orchestrator.
     * @return Returns null if no job is on the orchestrator.
     * @throws RemoteException
     */
    public JobData requestJob() throws RemoteException {
        return orchestrator.requestJob();
    }

    public boolean returnJob(JobData job) throws RemoteException {
        return false;
    }
}
