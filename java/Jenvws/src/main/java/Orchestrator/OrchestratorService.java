package Orchestrator;

import DataObjects.TrackerData;
import ServiceStubs.OrchestratorServiceStub;
import org.apache.log4j.Logger;

import javax.rmi.ssl.SslRMIClientSocketFactory;
import javax.rmi.ssl.SslRMIServerSocketFactory;
import java.net.InetAddress;
import java.rmi.registry.LocateRegistry;
import java.rmi.registry.Registry;
import java.rmi.server.RMIClientSocketFactory;
import java.rmi.server.RMIServerSocketFactory;
import java.rmi.server.UnicastRemoteObject;
import java.util.concurrent.ScheduledThreadPoolExecutor;

/**
 * @author jim
 * @class
 * @date 6/27/14
 */
public class OrchestratorService implements OrchestratorServiceStub {
    private Logger logger = Logger.getLogger(OrchestratorService.class.getName());

    private final int MAX_LATE_MILLIES = 2500;

    private TrackerManager trackerManager = new TrackerManager(MAX_LATE_MILLIES);

    private JobsManager jobsManager;


    private InetAddress rmiIp;
    private int rmiPort;

    public OrchestratorService(InetAddress rmiIp, int rmiPort, JobsManager jom, TrackerManager trm) {
        this.rmiIp = rmiIp;
        this.rmiPort = rmiPort;
        this.jobsManager = jom;
        this.trackerManager = trm;
    }

    public void bind(String name) {
        try {
            RMIClientSocketFactory rmiClientSocketFactory = new SslRMIClientSocketFactory();
            RMIServerSocketFactory rmiServerSocketFactory = new SslRMIServerSocketFactory();
            OrchestratorServiceStub ccAuth = (OrchestratorServiceStub) UnicastRemoteObject.exportObject(this, 0,
                    rmiClientSocketFactory, rmiServerSocketFactory);
            Registry registry = LocateRegistry.createRegistry(rmiPort);
            registry.rebind(name, ccAuth);
            logger.info(name + " bound in registry");
        } catch (Exception e) {
            logger.error("Unable to bind to the registry", e);
        }
    }

    //////////////////////////////////////////////
    //   R M I   H A N D L E R S                //
    //////////////////////////////////////////////

    @Override
    public boolean ping(TrackerData data) {
        return true;
    }


}
