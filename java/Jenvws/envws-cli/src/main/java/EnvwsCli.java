import DataObjects.TrackerData;
import ServiceStubs.OrchestratorServiceStub;
import org.apache.commons.cli.*;

import java.net.InetAddress;
import java.net.UnknownHostException;
import java.rmi.NotBoundException;
import java.rmi.RemoteException;
import java.rmi.registry.LocateRegistry;
import java.rmi.registry.Registry;
import java.util.Arrays;

public class EnvwsCli {

    private static String EmptyString = "";

    private void doList(String[] args) {
        Options listOpts = CLOptions.makeListOptions();
        CommandLineParser parser = new BasicParser();
        CommandLine line;

        try{
            line = parser.parse(listOpts, args);
            String host = EmptyString;
            int port = 12345;
            if (line.hasOption("orch")) {
                host = line.getOptionValue("orch");
            }

            if (line.hasOption("port")) {
                try{
                    String p = line.getOptionValue("port");
                    port = Integer.parseInt(p);
                } catch (NumberFormatException e) {
                    throw new ParseException("Bad value for port");
                }
            }

            InetAddress endpoint = InetAddress.getByName(host);
            TrackerData[] trackerDatas = lookupRegistry(endpoint, port).getAllTrackers();


        } catch (ParseException e) {
            System.err.println(e.getMessage());
            new HelpFormatter().printHelp(80, "envws-cli", ":D", listOpts, ":D");
            System.exit(1);
        } catch (UnknownHostException e) {
            e.printStackTrace();
        } catch (RemoteException e) {
            e.printStackTrace();
        } catch (NotBoundException e) {
            e.printStackTrace();
        }
    }

    private OrchestratorServiceStub lookupRegistry(InetAddress host, int port)
            throws RemoteException, NotBoundException {
        Registry registry = LocateRegistry.getRegistry(String.valueOf(host), port);
        return (OrchestratorServiceStub) registry.lookup("OrchestratorService");
    }

    public void doMain(String[] args) {
        Options generalOpts = CLOptions.makeOptions();
        CommandLineParser parser = new BasicParser();
        CommandLine line;

        try{
            line = parser.parse(generalOpts, args);
            if (line.hasOption("list")) {
                doList(Arrays.copyOfRange(args, 1, args.length-1));
            } else if (line.hasOption("submit")) {
//                doSubmit();
            } else {
                throw new ParseException("Not the args I am looking for.");
            }
        }catch (ParseException e) {
            System.err.println(e.getMessage());
            new HelpFormatter().printHelp(80, "envws-cli", ":D", generalOpts, ":D");
            System.exit(1);
        }
    }



    public static void main( String[] args ) {
        new EnvwsCli().doMain(args);
    }
}
