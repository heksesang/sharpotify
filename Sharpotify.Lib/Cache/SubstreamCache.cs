using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Sharpotify.Media;
using Sharpotify.Protocol.Channel;
using Sharpotify.Enums;

namespace Sharpotify.Cache
{
    internal class SubstreamCache : FileCache
    {
        #region methods

        public string Hash(Media.File file, int offset, int length)
        {
            return file.Id + "/" + offset + "-" + length;
        }

        public void Load(string category, string hash, IChannelListener listener)
        {
            /* Load data in a separate thread, because we're an asynchronous load method. */

            new Thread(delegate() { 
                Channel channel = new Channel("Cached-Substream-Channel", ChannelType.TYPE_SUBSTREAM, null);
                listener.ChannelHeader(channel, null);
				listener.ChannelData(channel, Load(category, hash));
				listener.ChannelEnd(channel);
            }).Start();
	    }

        #endregion

        #region construction

        public SubstreamCache() : base()
        {
        }

        public SubstreamCache(string directory) : base(directory)
        {
        }

        #endregion
    }
}
