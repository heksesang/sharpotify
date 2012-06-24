using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Protocol.Channel
{
    public interface IChannelListener
    {
        void ChannelHeader(Channel channel, byte[] header);
        void ChannelData(Channel channel, byte[] data);
        void ChannelError(Channel channel);
        void ChannelEnd(Channel channel);
    }
}
