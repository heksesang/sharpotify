using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Protocol.Channel
{
    internal class ChannelAdapter : IChannelListener
    {
        #region ChannelListener Members

        public void ChannelHeader(Channel channel, byte[] header)
        {
        }

        public void ChannelData(Channel channel, byte[] data)
        {
        }

        public void ChannelError(Channel channel)
        {
        }

        public void ChannelEnd(Channel channel)
        {
        }

        #endregion
    }
}
