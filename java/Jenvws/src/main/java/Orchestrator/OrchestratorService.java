package Orchestrator;

import DataObjects.JobData;
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

/**
 * @author jim
 * @class
 * @date 6/27/14
 */
public class OrchestratorService implements OrchestratorServiceStub {
    private Logger logger = Logger.getLogger(OrchestratorService.class.getName());

    private TrackerManager trackerManager;
    private JobsManager jobsManager;

    public OrchestratorService(JobsManager jom, TrackerManager trm) {
        this.jobsManager = jom;
        this.trackerManager = trm;
    }

    //////////////////////////////////////////////
    //   R M I   H A N D L E R S                //
    //////////////////////////////////////////////

    @Override
    public void submitJob(JobData job) {
        jobsManager.pushJob(job);
    }

    @Override
    public JobData[] getAllJobs() {
        return jobsManager.getAllJobs().toArray(new JobData[0]);
    }

    @Override
    public TrackerData[] getAllTrackers() {
        return new TrackerData[0];
    }
}
