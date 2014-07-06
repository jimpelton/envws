import org.apache.commons.cli.Option;
import org.apache.commons.cli.OptionBuilder;
import org.apache.commons.cli.OptionGroup;
import org.apache.commons.cli.Options;

/**
 * @author jim
 * @class
 * @date 7/5/14
 */
@SuppressWarnings("AccessStaticViaInstance")
public class CLOptions {

    protected static Options makeOptions() {
        Option submitJob = OptionBuilder
                .hasArg(false)
                .withDescription("Submit a job to the cluster.")
                .withLongOpt("submit")
                .create();

        Option listTrackers = OptionBuilder
                .hasArg(false)
                .withDescription("List the trackers and corresponding status")
                .withLongOpt("list")
                .create();

        Option removeJob = OptionBuilder
                .hasArg(false)
                .withDescription("Remove a job from the queue")
                .withLongOpt("remove")
                .create();

        Option viewQueue = OptionBuilder
                .hasArg(false)
                .withDescription("View the Job Queue")
                .withLongOpt("queue")
                .create();

        Options opts =
            new Options()
                .addOptionGroup(
                    new OptionGroup()
                    .addOption(submitJob)
                    .addOption(listTrackers)
                    .addOption(removeJob)
                    .addOption(viewQueue)
                );

        return opts;
    }

    protected static Options makeListOptions() {
        Option orch = OptionBuilder
                .isRequired(true)
                .hasArg(true)
                .withArgName("endpoint")
                .withDescription("The orchestrator hostname or IP")
                .withLongOpt("orch")
                .create("o");

        Option port = OptionBuilder
                .isRequired(true)
                .hasArg(true)
                .withArgName("port")
                .withDescription("The orchestrator port")
                .withLongOpt("port")
                .create("p");

        Options opts = new Options()
                .addOption(orch)
                .addOption(port);

        return opts;
    }

    protected static Options makeRemoveOptions() {
        throw new UnsupportedOperationException();
    }

    protected static Options makeViewQueueOptions() {
        throw new UnsupportedOperationException();
    }



}
