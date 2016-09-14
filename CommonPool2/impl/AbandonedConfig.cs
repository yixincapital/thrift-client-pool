using System.IO;

namespace CommonPool2.impl
{
    public class AbandonedConfig
    {
        
    /**
     * Whether or not borrowObject performs abandoned object removal.
     */
    private bool removeAbandonedOnBorrow = false;

    /**
     * <p>Flag to remove abandoned objects if they exceed the
     * removeAbandonedTimeout when borrowObject is invoked.</p>
     *
     * <p>The default value is false.</p>
     *
     * <p>If set to true, abandoned objects are removed by borrowObject if
     * there are fewer than 2 idle objects available in the pool and
     * <code>getNumActive() &gt; getMaxTotal() - 3</code></p>
     *
     * @return true if abandoned objects are to be removed by borrowObject
     */
    public bool getRemoveAbandonedOnBorrow() {
        return this.removeAbandonedOnBorrow;
    }

    /**
     * <p>Flag to remove abandoned objects if they exceed the
     * removeAbandonedTimeout when borrowObject is invoked.</p>
     *
     * @param removeAbandonedOnBorrow true means abandoned objects will be
     *   removed by borrowObject
     * @see #getRemoveAbandonedOnBorrow()
     */
    public void setRemoveAbandonedOnBorrow(bool removeAbandonedOnBorrow) {
        this.removeAbandonedOnBorrow = removeAbandonedOnBorrow;
    }

    /**
     * Whether or not pool maintenance (evictor) performs abandoned object
     * removal.
     */
    private bool removeAbandonedOnMaintenance = false;

    /**
     * <p>Flag to remove abandoned objects if they exceed the
     * removeAbandonedTimeout when pool maintenance (the "evictor")
     * runs.</p>
     *
     * <p>The default value is false.</p>
     *
     * <p>If set to true, abandoned objects are removed by the pool
     * maintenance thread when it runs.  This setting has no effect
     * unless maintenance is enabled by setting
     *{@link GenericObjectPool#getTimeBetweenEvictionRunsMillis() timeBetweenEvictionRunsMillis}
     * to a positive number.</p>
     *
     * @return true if abandoned objects are to be removed by the evictor
     */
    public bool getRemoveAbandonedOnMaintenance() {
        return this.removeAbandonedOnMaintenance;
    }

    /**
     * <p>Flag to remove abandoned objects if they exceed the
     * removeAbandonedTimeout when pool maintenance runs.</p>
     *
     * @param removeAbandonedOnMaintenance true means abandoned objects will be
     *   removed by pool maintenance
     * @see #getRemoveAbandonedOnMaintenance
     */
    public void setRemoveAbandonedOnMaintenance(bool removeAbandonedOnMaintenance) {
        this.removeAbandonedOnMaintenance = removeAbandonedOnMaintenance;
    }

    /**
     * Timeout in seconds before an abandoned object can be removed.
     */
    private int removeAbandonedTimeout = 300;

    /**
     * <p>Timeout in seconds before an abandoned object can be removed.</p>
     *
     * <p>The time of most recent use of an object is the maximum (latest) of
     * {@link TrackedUse#getLastUsed()} (if this class of the object implements
     * TrackedUse) and the time when the object was borrowed from the pool.</p>
     *
     * <p>The default value is 300 seconds.</p>
     *
     * @return the abandoned object timeout in seconds
     */
    public int getRemoveAbandonedTimeout() {
        return this.removeAbandonedTimeout;
    }

    /**
     * <p>Sets the timeout in seconds before an abandoned object can be
     * removed</p>
     *
     * <p>Setting this property has no effect if
     * {@link #getRemoveAbandonedOnBorrow() removeAbandonedOnBorrow} and
     * {@link #getRemoveAbandonedOnMaintenance() removeAbandonedOnMaintenance}
     * are both false.</p>
     *
     * @param removeAbandonedTimeout new abandoned timeout in seconds
     * @see #getRemoveAbandonedTimeout()
     */
    public void setRemoveAbandonedTimeout(int removeAbandonedTimeout) {
        this.removeAbandonedTimeout = removeAbandonedTimeout;
    }

    /**
     * Determines whether or not to log stack traces for application code
     * which abandoned an object.
     */
    private bool logAbandoned = false;

    /**
     * Flag to log stack traces for application code which abandoned
     * an object.
     *
     * Defaults to false.
     * Logging of abandoned objects adds overhead for every object created
     * because a stack trace has to be generated.
     *
     * @return bool true if stack trace logging is turned on for abandoned
     * objects
     *
     */
    public bool getLogAbandoned() {
        return this.logAbandoned;
    }

    /**
     * Sets the flag to log stack traces for application code which abandoned
     * an object.
     *
     * @param logAbandoned true turns on abandoned stack trace logging
     * @see #getLogAbandoned()
     *
     */
    public void setLogAbandoned(bool logAbandoned) {
        this.logAbandoned = logAbandoned;
    }

    /**
     * PrintWriter to use to log information on abandoned objects.
     * Use of default system encoding is deliberate.
     */
  //  private PrintWriter logWriter = new PrintWriter(System.out);

    /**
     * Returns the log writer being used by this configuration to log
     * information on abandoned objects. If not set, a PrintWriter based on
     * System.out with the system default encoding is used.
     *
     * @return log writer in use
     */
    public StringWriter getLogWriter() {
        return null;
    }

    /**
     * Sets the log writer to be used by this configuration to log
     * information on abandoned objects.
     *
     * @param logWriter The new log writer
     */
    public void setLogWriter(StringWriter logWriter) {
       // this.logWriter = logWriter;
    }

    /**
     * If the pool implements {@link UsageTracking}, should the pool record a
     * stack trace every time a method is called on a pooled object and retain
     * the most recent stack trace to aid debugging of abandoned objects?
     */
    private bool useUsageTracking = false;

    /**
     * If the pool implements {@link UsageTracking}, should the pool record a
     * stack trace every time a method is called on a pooled object and retain
     * the most recent stack trace to aid debugging of abandoned objects?
     *
     * @return <code>true</code> if usage tracking is enabled
     */
    public bool getUseUsageTracking() {
        return useUsageTracking;
    }

    /**
     * If the pool implements {@link UsageTracking}, configure whether the pool
     * should record a stack trace every time a method is called on a pooled
     * object and retain the most recent stack trace to aid debugging of
     * abandoned objects.
     *
     * @param   useUsageTracking    A value of <code>true</code> will enable
     *                              the recording of a stack trace on every use
     *                              of a pooled object
     */
    public void setUseUsageTracking(bool useUsageTracking) {
        this.useUsageTracking = useUsageTracking;
    } 
    }
}