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

    private static final int MAX_THREAD_POOL_SIZE = 10;

    private static ScheduledThreadPoolExecutor executor = new ScheduledThreadPoolExecutor(MAX_THREAD_POOL_SIZE);
    static {
        executor.schedule(JobScheduler::scheduledFuturesListScrubber, 5000, TimeUnit.MILLISECONDS);
    }

    private static ArrayList<ScheduledFuture<?>> scheduledFutures  = new ArrayList<>();

    private static void scheduledFuturesListScrubber() {
        scheduledFutures.removeIf(v -> v.isDone());
    }

    public static int submitRecurring(Runnable c, int initialDelayMillis, int repeatAfterMillis) {
        ScheduledFuture fu = executor.scheduleWithFixedDelay(() -> {

            try {
                c.run();
            } catch (Exception e) {
                logger.error(e);
            }

        }, initialDelayMillis, repeatAfterMillis, TimeUnit.MILLISECONDS);

        scheduledFutures.add(fu);

        return scheduledFutures.size()-1;
    }

    public static int submitOneShot(Callable c, int delayMillis) {
        ScheduledFuture fu = executor.schedule(c, delayMillis, TimeUnit.MILLISECONDS);
        scheduledFutures.add(fu);

        return scheduledFutures.size()-1;
    }

    public static int submitOneShot(Runnable c, int delayMillis) {
        ScheduledFuture fu = executor.schedule(c, delayMillis, TimeUnit.MILLISECONDS);
        scheduledFutures.add(fu);

        return scheduledFutures.size()-1;
    }

    public static void cancel(int index) {
        scheduledFutures.get(index).cancel(false);
    }
}
