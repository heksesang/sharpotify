using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharpotify.Util;
using System.IO;

namespace Sharpotify.Protocol.Channel
{
    internal class ChannelCallback : IChannelListener
    {
        #region Fields
        private Semaphore _done = new Semaphore(1); //FIXME: Dispose after use
        private List<ByteBuffer> _buffers = new List<ByteBuffer>();
        private int _bytes = 0;
        #endregion
        #region Properties
        public bool IsDone
        {
            get
            {
                return _done.AvailablePermits > 0;
            }
        }
        #endregion
        #region Factory
        public ChannelCallback()
        {
            this._done.AcquireUninterruptibly();
        }

        #endregion

        #region methods
        public byte[] GetData()
        {
            ByteBuffer data = ByteBuffer.Allocate(this._bytes);
		
		    foreach (ByteBuffer b in this._buffers)
            {
			    data.Put(b.ToArray());
		    }
    		
		    /* Get data bytes. */
		    byte[] bytes = data.ToArray();

            if (bytes.Length > 0)
            {
                /* Detect GZIP magic and return inflated data. */
                if (bytes[0] == (byte)0x1f && bytes[1] == (byte)0x8b)
                {
                    return GZIP.Inflate(bytes);
                }
            }
		    /* Return data. */
		    return bytes;
        }

        public byte[] Get()
        {
            /* Wait for data to become available. */
            this._done.AcquireUninterruptibly();

            /* Return data array. */
            return this.GetData();
        }

        public byte[] Get(TimeSpan timeout)
        {
            /* Wait for data to become available. */
            if (!this._done.TryAcquire(timeout))
            {
                throw new TimeoutException("Timeout while waiting for data.");
            }

            return this.GetData();
        }

        #endregion
        #region ChannelListener Members
        public void ChannelHeader(Channel channel, byte[] header)
        {
            /* Ignore */
        }

        public void ChannelData(Channel channel, byte[] data)
        {
            ByteBuffer buffer = new ByteBuffer(data);
            this._bytes += data.Length;
            this._buffers.Add(buffer);
        }

        public void ChannelError(Channel channel)
        {
            this._done.Release();
        }

        public void ChannelEnd(Channel channel)
        {
            Channel.Unregister(channel.Id);
            this._done.Release();
        }
        #endregion
    }
}
