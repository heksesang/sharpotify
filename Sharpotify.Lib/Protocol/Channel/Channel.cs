using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sharpotify.Util;

namespace Sharpotify.Protocol.Channel
{
    public class Channel
    {
        #region Fields
        /* Static channel id counter. */
        private static int _nextId = 0;
        private static Dictionary<int, Channel> _channels = new Dictionary<int, Channel>();
        /* Channel variables. */
        private int _id;
        private string _name;
        private ChannelState _state;
        private ChannelType _type;
        private int _headerLength;
        private int _dataLength;
        private IChannelListener _listener;
        #endregion
        #region Properties
        public int Id { get { return _id; } }
        public string Name { get { return _name; } }
        public ChannelState State { get { return _state; } }
        public ChannelType Type { get { return _type; } }
        public int HeaderLength { get { return _headerLength; } }
        public int DataLength { get { return _dataLength; } }
        #endregion
        #region Factory
        public Channel(string name, ChannelType type, IChannelListener listener)
        {
            this._id = Channel._nextId++;
            this._name = name + "-" + this.Id;
            this._state = ChannelState.STATE_HEADER;
            this._type = type;
            this._headerLength = 0;
            this._dataLength = 0;
            this._listener = listener;

            /* Force data state for AES key channel. */
            if (this._type == ChannelType.TYPE_AESKEY)
                this._state = ChannelState.STATE_DATA;
        }
        #endregion
        #region Methods
        public static void Register(Channel channel)
        {
		    Channel._channels.Add(channel.Id, channel);
	    }
    	
	    public static void Unregister(int id)
        {
		    Channel._channels.Remove(id);
	    }

        public static void Process(byte[] payload)
        {
            Channel channel;
            int offset = 0;
            int length = payload.Length;
            int headerLength = 0;
            int consumedLength = 0;

            /* Get Channel by id from payload. */
            if ((channel = Channel._channels[ShortUtilities.BytesToUnsignedShort(payload)]) == null)
            {
                /* Just return if channel is not registered. */
                return;
            }

            offset += 2;
            length -= 2;

            if (channel.State == ChannelState.STATE_HEADER)
            {
                if (length < 2)
                {
                    System.Console.WriteLine("Length is smaller than 2!");
                    return;
                }

                while (consumedLength < length)
                {
                    /* Extract length of next data. */
                    headerLength = ShortUtilities.BytesToUnsignedShort(payload, offset);

                    offset += 2;
                    consumedLength += 2;

                    if (headerLength == 0)
                    {
                        break;
                    }

                    if (consumedLength + headerLength > length)
                    {
                        System.Console.WriteLine("Not enough data.");
                        return;
                    }

                    if (channel._listener != null)
                    {
                        byte[] buffer = new byte[headerLength];
                        Array.Copy(payload, offset, buffer, 0, headerLength);

                        channel._listener.ChannelHeader(channel, buffer);
                    }

                    offset += headerLength;
                    consumedLength += headerLength;

                    channel._headerLength += headerLength;
                }

                if (consumedLength != length)
                {
                    System.Console.WriteLine("Didn't consume all data!");
                    return;
                }

                /* Upgrade state if this was the last (zero size) header. */
                if (headerLength == 0)
                {
                    channel._state = ChannelState.STATE_DATA;
                }

                return;
            }

            /*
             * Now we're either in the CHANNEL_DATA or CHANNEL_ERROR state.
             * If in CHANNEL_DATA and length is zero, switch to CHANNEL_END,
             * thus letting the callback routine know this is the last packet.
             */
            if (length == 0)
            {
                channel._state = ChannelState.STATE_END;

                if (channel._listener != null)
                {
                    channel._listener.ChannelEnd(channel);
                }
            }
            else
            {
                if (channel._listener != null)
                {
                    byte[] buffer = new byte[length];
                    Array.Copy(payload, offset, buffer, 0, length);
                    channel._listener.ChannelData(channel, buffer);
                }
            }

            channel._dataLength += length;

            /* If this is an AES key channel, force end state. */
            if (channel.Type == ChannelType.TYPE_AESKEY)
            {
                channel._state = ChannelState.STATE_END;

                if (channel._listener != null)
                {
                    channel._listener.ChannelEnd(channel);
                }
            }
        }

        public static void Error(byte[] payload)
        {
            Channel channel;

            /* Get Channel by id from payload. */
            if ((channel = Channel._channels[ShortUtilities.BytesToUnsignedShort(payload)]) == null)
            {
                System.Console.WriteLine("Channel not found!");
                return;
            }

            if (channel._listener != null)
            {
                channel._listener.ChannelError(channel);
            }

            Channel._channels.Remove(channel.Id);
        }
        #endregion
    }
}
