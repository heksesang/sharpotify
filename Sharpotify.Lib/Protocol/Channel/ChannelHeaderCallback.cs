using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharpotify.Util;

namespace Sharpotify.Protocol.Channel
{
    internal class ChannelHeaderCallback : IChannelListener
    {
        #region fields

        private Semaphore _done = new Semaphore(1); //FIXME: Dispose after use
        private List<string> _urls = new List<string>();

        #endregion


        #region properties

        public bool IsDone
        {
            get
            {
                return _done.AvailablePermits > 0;
            }
        }

        #endregion


        #region methods

        public string[] GetData()
        {
            return _urls.ToArray();
        }

        public string[] Get()
        {
            /* Wait for data to become available. */
            this._done.AcquireUninterruptibly();

            /* Return HTTP stream URLs. */
            return this.GetData();
        }

        public string[] Get(TimeSpan timeout)
        {
            /* Wait for data to become available. */
            if (!this._done.TryAcquire(timeout))
            {
                throw new TimeoutException("Timeout while waiting for data.");
            }

            /* Return HTTP stream URLs. */
            return this.GetData();
        }

        #endregion


        #region construction

        public ChannelHeaderCallback()
        {
            this._done.AcquireUninterruptibly();
        }

        #endregion


        #region ChannelListener Members

        public void ChannelHeader(Channel channel, byte[] header)
        {
            /* HTTP stream. */
            if (header[0] == 0x07)
            {
                /* header[1] contains number of stream part. */
                this._urls.Add(Encoding.UTF8.GetString(header, 2, header.Length - 2));
            }
        }

        public void ChannelData(Channel channel, byte[] data)
        {
            /* Ignore */
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
