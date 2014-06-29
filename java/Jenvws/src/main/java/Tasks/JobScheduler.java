package Tasks;

import org.apache.log4j.LogManager;
import org.apache.log4j.Logger;

import java.util.ArrayList;
import java.util.concurrent.Callable;
import java.util.concurrent.ScheduledFuture;
import java.util.concurrent.ScheduledThreadPoolExecutor;
import java.util.concurrent.TimeUnit;

/**
 * @author jim
 * @class
 * @date 6/29/14
 */
public class JobScheduler {

    private static Logger logger = LogManager.getLogger(JobScheduler.class.getName());

    private final int MAX_THREAD_POOL_SIZE = 10;
    private ScheduledThreadPoolExecutor executor = new ScheduledThreadPoolExecutor(MAX_THREAD_POOL_SIZE);
    private static JobScheduler myself = null;

    private ArrayList<ScheduledFuture<?>> scheduledFutures  = new ArrayList<>();

    public static int submitRecurring(Callable<Boolean> c, int initialDelayMillis, int repeatAfterMillis) {
        if (myself == null) myself = new JobScheduler();

        ScheduledFuture fu = myself.executor.scheduleAtFixedRate(() -> {

            try {
                c.call();
            } catch (Exception e) {
                logger.error(e);
            }

        }, initialDelayMillis, repeatAfterMillis, TimeUnit.MILLISECONDS);

        myself.scheduledFutures.add(fu);

        return myself.scheduledFutures.size()-1;
    }

    public static int submitOneShot(Callable<Boolean> c, int delayMillis) {
        if (myself == null) myself = new JobScheduler();

        ScheduledFuture<Boolean> fu = myself.executor.schedule(c, delayMillis, TimeUnit.MILLISECONDS);

        myself.scheduledFutures.add(fu);

        return myself.scheduledFutures.size()-1;
    }

    public static void cancel(int index) {
        if (myself==null) myself=new JobScheduler();

        myself.scheduledFutures.get(index).cancel(false);
    }
}
