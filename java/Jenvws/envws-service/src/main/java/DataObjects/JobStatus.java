package DataObjects;

/**
 * @author jim
 * @class
 * @date 6/29/14
 */
public enum JobStatus {
    QUEUED, IN_TRANSIT, RUNNING,
    RETURNED_UNFINISHED, RETURNED_FINISHED,
    UNRETURNED_UNFINISHED, UNRETURNED_FINISHED, NONE
}
