import Orchestrator.JobRequestService;
import Orchestrator.JobsManager;
import Orchestrator.OrchestratorService;
import Orchestrator.TrackerManager;
import ServiceStubs.JobRequestServiceStub;
import ServiceStubs.OrchestratorServiceStub;
import Tracker.Tracker;

import org.apache.commons.cli.*;
import org.apache.log4j.BasicConfigurator;
import org.apache.log4j.LogManager;
import org.apache.log4j.Logger;

import java.net.InetAddress;
import java.net.UnknownHostException;
import java.rmi.NotBoundException;
import java.rmi.RemoteException;
import java.rmi.registry.LocateRegistry;
import java.rmi.registry.Registry;
import java.rmi.server.UnicastRemoteObject;


/**
 * @author jim
 * @class
 * @date 6/27/14
 */

@SuppressWarnings("ALL")
public class Jenvws {
    private static final Logger logger = LogManager.getLogger(Jenvws.class.getName());

    private static final String EmptyString = "";

    private String nodeType = EmptyString;

    private Options makeGeneralOptions() {

        Option help = new Option("h", "help", false, "print this help message");
        Option version = new Option("v", "version", false, "print version information");

        Option nodeType = OptionBuilder
                .withArgName("node type")
                .hasArg()
                .isRequired()
                .withDescription("orch or tracker")
                .create("type");

        Options options = new Options();
        options.addOption(nodeType);

        return options;
    }

    private Options makeOrchestratorOptions() {

        Option rmiIp = OptionBuilder
                .withArgName("ip addr")
                .hasArg()
                .withDescription("The ip address of the rmi server.")
                .create("rmiIp");

        Option rmiPort = OptionBuilder
                .withArgName("port")
                .hasArg()
                .withDescription("Port for rmi server")
                .create("rmiPort");

        Options orchOpts = new Options();
        orchOpts.addOption(rmiIp).addOption(rmiPort);

        return orchOpts;
    }

    private Options makeTrackerOptions() {

        Option orchIp  = OptionBuilder
                .withArgName("ip addr")
                .hasArg()
                .withDescription("The ip address of the orchestrator.")
                .create("orchIp");

        Option orchPort = OptionBuilder
                .withArgName("port")
                .hasArg()
                .withDescription("Port for orchestrator.")
                .create("orchPort");

        Options trackOpts = new Options();
        trackOpts.addOption(orchIp).addOption(orchPort);

        return trackOpts;
    }

    private void doOrchestrator(CommandLine line) {

        logger.info("Starting orchestrator.");

        try {

            String rmiIp = EmptyString,
                   rmiPort = EmptyString;

            if (line.hasOption("rmiIp")) {
                rmiIp = line.getOptionValue("rmiIp");
            }

            if (line.hasOption("rmiPort")) {
                rmiPort = line.getOptionValue("rmiPort");
            }

            InetAddress ipAddr;
            if (!rmiIp.equals(EmptyString)) {
                ipAddr = InetAddress.getByName(rmiIp);
            } else {
                ipAddr = InetAddress.getLocalHost();
            }

            int portNumber = 12290;
            if (!rmiPort.equals(EmptyString)) {
                try{
                    portNumber = Integer.parseInt(rmiPort);
                } catch (NumberFormatException e) {
                    String msg = String.format(
                            "Invalid port given for rmiPort: %s. Using default: %d",
                            rmiPort, portNumber);
                    logger.error(msg);
                }
            }

            logger.info(String.format("RMI IP: %s, RMI PORT: %d", ipAddr.toString(), portNumber));

            JobsManager jom = new JobsManager();
            TrackerManager trm = new TrackerManager(1000, 3000, 6000);
            OrchestratorService orchService = new OrchestratorService(jom, trm);
            JobRequestService jobService = new JobRequestService(jom, trm);

//            RMIClientSocketFactory rmiClientSocketFactory = new RMIClientSocketFactory();
//            RMIServerSocketFactory rmiServerSocketFactory = new RMIServerSocketFactory();

            OrchestratorServiceStub orchStub = (OrchestratorServiceStub) UnicastRemoteObject
                    .exportObject(orchService, 0 /*, rmiClientSocketFactory, rmiServerSocketFactory*/);

            JobRequestServiceStub requestStub = (JobRequestServiceStub) UnicastRemoteObject
                    .exportObject(jobService, 0 /*, rmiClientSocketFactory, rmiServerSocketFactory*/);

            Registry registry = LocateRegistry.createRegistry(portNumber);
            registry.rebind("OrchestratorService", orchStub);
            registry.rebind("JobRequestService", requestStub);

            logger.info("reached end of binding stuff");

        } catch (UnknownHostException e) {
            logger.fatal("Could not find host given for RMI IP.", e);
            System.exit(0);
        } catch (Exception e) {
            logger.fatal("Exception in doOrchestrator().", e);
            System.exit(0);
        }

    }

    private void doTracker(CommandLine line) {

        logger.info("Starting tracker.");

        try {

            String orchIp = EmptyString, orchPort = EmptyString;
            InetAddress ipAddr;
            int portNumber;

            if (line.hasOption("orchIp")) {
                orchIp = line.getOptionValue("orchIp");
            }

            if (line.hasOption("orchPort")) {
                orchPort = line.getOptionValue("orchPort");
            }

            if (!orchIp.equals(EmptyString)) {
                ipAddr = InetAddress.getByName(orchIp);
            } else {
                ipAddr = InetAddress.getLocalHost();
            }

            if (!orchPort.equals(EmptyString)) {
                portNumber = Integer.parseInt(orchPort);
            } else {
                portNumber = 12290;
            }

            String hostname = InetAddress.getLocalHost().getHostName().toString();
            Tracker tr = new Tracker(hostname, ipAddr, portNumber);
            tr.findRegistryAndCreateStub();
            tr.startPingLoop();


        } catch (UnknownHostException e) {
            logger.fatal("Could not find host given for RMI IP.", e);
            System.exit(0);
        } catch (NumberFormatException e) {
            logger.fatal("That was such a bad port number!", e);
            System.exit(0);
        } catch (RemoteException e) {
            e.printStackTrace();
        } catch (NotBoundException e) {
            e.printStackTrace();
        }
    }



    public void doMain(String[] args) {

        CommandLineParser parser = new BasicParser();
        CommandLine line;
        Options options = makeGeneralOptions();

        for (Object o : makeOrchestratorOptions().getOptions())
            options.addOption((Option) o);

        for (Object o : makeTrackerOptions().getOptions())
            options.addOption((Option) o);


        try {

            line = parser.parse(options, args);

            if (line.hasOption("type")) {
                nodeType = line.getOptionValue("type");
            }

            if (nodeType.equals("orch"))
                doOrchestrator(line);
            else if (nodeType.equals("tracker"))
                doTracker(line);

        } catch (ParseException e) {
            System.err.println(e.getMessage());
            new HelpFormatter().printHelp(80, "jenvws", ":D", options, ":D");
            System.exit(1);
        } catch (Exception e) {
            logger.fatal("peep!", e);
            System.exit(1);
        }
    }

    public static void main(String[] args) {

        BasicConfigurator.configure();
        new Jenvws().doMain(args);

    }


}
