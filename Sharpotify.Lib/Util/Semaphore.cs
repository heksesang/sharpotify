namespace Sharpotify.Util
{
    using System;
    using System.Threading;

    internal class Semaphore
    {
        private int _availableCount;
        private readonly object _lock;
        private int _maxCount;

        public Semaphore(int maxCount) : this(maxCount, maxCount)
        {
        }

        public Semaphore(int availableCount, int maxCount)
        {
            this._availableCount = 1;
            this._maxCount = 1;
            this._lock = new object();
            if (maxCount < 1)
            {
                throw new ArgumentOutOfRangeException("maxCount", "Max count must be >= 1.");
            }
            this._availableCount = availableCount;
            this._maxCount = maxCount;
        }

        public void AcquireUninterruptibly()
        {
            lock (this._lock)
            {
                while (this._availableCount == 0)
                {
                    try
                    {
                        Monitor.Wait(this._lock);
                    }
                    catch (Exception)
                    {
                        Monitor.Pulse(this._lock);
                        throw;
                    }
                }
                this._availableCount--;
            }
        }

        public void Release()
        {
            this.Release(1);
        }

        public void Release(int releaseCount)
        {
            if (releaseCount < 1)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            lock (this._lock)
            {
                if ((this._availableCount + releaseCount) > this._maxCount)
                {
                    throw new Exception("Can't release that many.");
                }
                this._availableCount += releaseCount;
                Monitor.PulseAll(this._lock);
            }
        }

        public bool TryAcquire(TimeSpan timeout)
        {
            return this.TryAcquire(1, timeout);
        }

        public bool TryAcquire(int acquireCount, TimeSpan timeout)
        {
            lock (this._lock)
            {
                while (this._availableCount < acquireCount)
                {
                    try
                    {
                        if (!Monitor.Wait(this._lock, timeout))
                        {
                            return false;
                        }
                    }
                    catch (Exception)
                    {
                        Monitor.Pulse(this._lock);
                        throw;
                    }
                }
                this._availableCount -= acquireCount;
                return true;
            }
        }

        public int AvailablePermits
        {
            get
            {
                return this._availableCount;
            }
        }
    }
}

