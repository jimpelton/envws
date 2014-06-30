package Tracker;

import DataObjects.JobData;

/**
 *
 */
public class TrackerJobData {
    private boolean hasBeenReturnedToTracker = false;
    private JobData theJobInQuestion = JobData.emptyJob();

    public TrackerJobData() { }

    public boolean isHasBeenReturnedToTracker() {
        return hasBeenReturnedToTracker;
    }

    public void setHasBeenReturnedToTracker(boolean hasBeenReturnedToTracker) {
        this.hasBeenReturnedToTracker = hasBeenReturnedToTracker;
    }

    public JobData getTheJobInQuestion() {
        return theJobInQuestion;
    }

    public void setTheJobInQuestion(JobData theJobInQuestion) {
        this.theJobInQuestion = theJobInQuestion;
    }
}
