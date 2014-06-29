import Orchestrator.JobsManager;
import Orchestrator.OrchestratorService;
import Orchestrator.TrackerManager;
import com.sun.xml.internal.ws.api.streaming.XMLStreamReaderFactory;
import org.apache.commons.cli.*;
import org.apache.log4j.BasicConfigurator;
import org.apache.log4j.LogManager;
import org.apache.log4j.Logger;


import java.net.InetAddress;
import java.net.UnknownHostException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * @author jim
 * @class
 * @date 6/27/14
 */

@SuppressWarnings("ALL")
public class Jenvws {
    private static final String EmptyString = "";
    
    private static final Logger logger
            = LogManager.getLogger(Jenvws.class.getName());

    private String nodeType = EmptyString;
    private String rmiIp    = EmptyString;
    private String rmiPort  = EmptyString;


    private Options makeOptions() {
        Option help = new Option("help", "print this help message");
        Option version = new Option("version", "print version information");

        Option nodeType = OptionBuilder
                .withArgName("node type")
                .hasArg()
                .isRequired()
                .withDescription("orch or tracker")
                .withLongOpt("nodetype")
                .create("t");

        Option rmiIp = OptionBuilder
                .withArgName("ip addr")
                .hasArg()
                .isRequired()
                .withDescription("The ip address of the rmi server.")
                .withLongOpt("rmiIp")
                .create("r");

        Option rmiPort = OptionBuilder
                .withArgName("port")
                .hasArg()
                .withDescription("Port for rmi server")
                .withLongOpt("rmiPort")
                .create("p");

        Options options = new Options();
        options.addOption(nodeType).addOption(rmiIp).addOption(rmiPort);

        return options;
    }

    private void doOrchestrator() {
        logger.info("Starting orchestrator.");
        try {
            InetAddress ipAddr;
            int portNumber;
            if (!rmiIp.equals(EmptyString)) {
                ipAddr = InetAddress.getByName(rmiIp);
            } else {
                ipAddr = InetAddress.getByName("localhost");
            }
            if (!rmiPort.equals(EmptyString)) {
                portNumber = Integer.parseInt(rmiPort);
            } else {
                portNumber = 12290;
            }

            JobsManager jom = new JobsManager();
            TrackerManager trm = new TrackerManager(1000);
            OrchestratorService service = new OrchestratorService(ipAddr, portNumber, jom, trm);

            service.bind("OrchestratorService");
            logger.info("Bound to rmi registry");

        } catch (UnknownHostException e) {
            logger.fatal("Could not find host given for RMI IP.", e);
            System.exit(0);
        } catch (Exception e) {
            logger.fatal("Exception in doOrchestrator().", e);
            System.exit(0);
        }

    }

    private void doTracker() {
        logger.info("Starting tracker.");
    }

    public static void main(String[] args) {
        BasicConfigurator.configure();
        new Jenvws().doMain(args);
    }

    public void doMain(String[] args) {
        CommandLineParser parser = new BasicParser();
        CommandLine line;
        try{
            line = parser.parse(makeOptions(), args);
            if (line.hasOption('t')) {
                nodeType = line.getOptionValue('t');
            }

            if (line.hasOption('r')) {
                rmiIp = line.getOptionValue('r');
            }

            if (line.hasOption('p')) {
                rmiPort = line.getOptionValue('p');
            }
        } catch (ParseException e) {
            logger.fatal("Bad cmd options!");
            System.exit(0);
        }

        if (nodeType.equals("orch"))
            doOrchestrator();
        else if (nodeType.equals("tracker"))
            doTracker();
    }
}
