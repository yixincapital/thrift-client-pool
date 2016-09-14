using System;

namespace CommonPool2.impl
{
    public class GenericKeyedObjectPoolConfig:BaseObjectPoolConfig
    {
         /**
     * The default value for the {@code maxTotalPerKey} configuration attribute.
     * @see GenericKeyedObjectPool#getMaxTotalPerKey()
     */
    public static readonly int DEFAULT_MAX_TOTAL_PER_KEY = 8;

    /**
     * The default value for the {@code maxTotal} configuration attribute.
     * @see GenericKeyedObjectPool#getMaxTotal()
     */
    public static readonly int DEFAULT_MAX_TOTAL = -1;

    /**
     * The default value for the {@code minIdlePerKey} configuration attribute.
     * @see GenericKeyedObjectPool#getMinIdlePerKey()
     */
    public static readonly int DEFAULT_MIN_IDLE_PER_KEY = 0;

    /**
     * The default value for the {@code maxIdlePerKey} configuration attribute.
     * @see GenericKeyedObjectPool#getMaxIdlePerKey()
     */
    public static readonly int DEFAULT_MAX_IDLE_PER_KEY = 8;


    private int minIdlePerKey = DEFAULT_MIN_IDLE_PER_KEY;

    private int maxIdlePerKey = DEFAULT_MAX_IDLE_PER_KEY;

    private int maxTotalPerKey = DEFAULT_MAX_TOTAL_PER_KEY;

    private int maxTotal = DEFAULT_MAX_TOTAL;

    /**
     * Create a new configuration with default settings.
     */
    public GenericKeyedObjectPoolConfig() {
    }

    /**
     * Get the value for the {@code maxTotal} configuration attribute
     * for pools created with this configuration instance.
     *
     * @return  The current setting of {@code maxTotal} for this
     *          configuration instance
     *
     * @see GenericKeyedObjectPool#getMaxTotal()
     */
    public int GetMaxTotal() {
        return maxTotal;
    }

    /**
     * Set the value for the {@code maxTotal} configuration attribute for
     * pools created with this configuration instance.
     *
     * @param maxTotal The new setting of {@code maxTotal}
     *        for this configuration instance
     *
     * @see GenericKeyedObjectPool#setMaxTotal(int)
     */
    public void SetMaxTotal(int maxTotal) {
        this.maxTotal = maxTotal;
    }

    /**
     * Get the value for the {@code maxTotalPerKey} configuration attribute
     * for pools created with this configuration instance.
     *
     * @return  The current setting of {@code maxTotalPerKey} for this
     *          configuration instance
     *
     * @see GenericKeyedObjectPool#getMaxTotalPerKey()
     */
    public int GetMaxTotalPerKey() {
        return maxTotalPerKey;
    }

    /**
     * Set the value for the {@code maxTotalPerKey} configuration attribute for
     * pools created with this configuration instance.
     *
     * @param maxTotalPerKey The new setting of {@code maxTotalPerKey}
     *        for this configuration instance
     *
     * @see GenericKeyedObjectPool#setMaxTotalPerKey(int)
     */
    public void SetMaxTotalPerKey(int maxTotalPerKey) {
        this.maxTotalPerKey = maxTotalPerKey;
    }

    /**
     * Get the value for the {@code minIdlePerKey} configuration attribute
     * for pools created with this configuration instance.
     *
     * @return  The current setting of {@code minIdlePerKey} for this
     *          configuration instance
     *
     * @see GenericKeyedObjectPool#getMinIdlePerKey()
     */
    public int GetMinIdlePerKey() {
        return minIdlePerKey;
    }

    /**
     * Set the value for the {@code minIdlePerKey} configuration attribute for
     * pools created with this configuration instance.
     *
     * @param minIdlePerKey The new setting of {@code minIdlePerKey}
     *        for this configuration instance
     *
     * @see GenericKeyedObjectPool#setMinIdlePerKey(int)
     */
    public void SetMinIdlePerKey(int minIdlePerKey) {
        this.minIdlePerKey = minIdlePerKey;
    }

    /**
     * Get the value for the {@code maxIdlePerKey} configuration attribute
     * for pools created with this configuration instance.
     *
     * @return  The current setting of {@code maxIdlePerKey} for this
     *          configuration instance
     *
     * @see GenericKeyedObjectPool#getMaxIdlePerKey()
     */
    public int GetMaxIdlePerKey() {
        return maxIdlePerKey;
    }

    /**
     * Set the value for the {@code maxIdlePerKey} configuration attribute for
     * pools created with this configuration instance.
     *
     * @param maxIdlePerKey The new setting of {@code maxIdlePerKey}
     *        for this configuration instance
     *
     * @see GenericKeyedObjectPool#setMaxIdlePerKey(int)
     */
    public void SetMaxIdlePerKey(int maxIdlePerKey) {
        this.maxIdlePerKey = maxIdlePerKey;
    }


        public override object Clone()
        {
           throw new NotImplementedException();
        }   
    }
}