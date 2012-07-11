using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.CompilerServices;

using Sharpotify.Util;
using Sharpotify.Exceptions;
using Sharpotify.Crypto;
using Sharpotify.Enums;
using Sharpotify.Protocol.Channel;
using Sharpotify.Media;
using System.Threading;

namespace Sharpotify.Protocol
{
    public class Protocol
    {
        #region Fields
        /* Socket connection to Spotify server. */
        private TcpClient client;
        /* Network Stream */
        private NetworkStream ioStream;

        /* Current server and port */
        private HostnamePortPair server;

        /* Spotify session of this protocol instance. */
        private Session session;

        /* Protocol listeners. */
        private List<ICommandListener> listeners;

        /* Server list */
        private static List<HostnamePortPair> servers;
        #endregion
        #region Factory
        /// <summary>
        /// Create a new protocol object.
        /// </summary>
        /// <param name="session"></param>
        public Protocol(Session session)
        {
            this.session = session;
            this.listeners = new List<ICommandListener>();
        }
        #endregion
        #region Methods
        /// <summary>
        /// Connect to one of the spotify servers.
        /// </summary>
        public void Connect()
        {
            if (servers == null)
            {
                /* Lookup servers via DNS SRV query. */
                servers = DNS.LookupSRV("_spotify-client._tcp.spotify.com");


                /* Add fallback servers if others don't work. */
                servers.Add(new HostnamePortPair("ap.spotify.com", 4070));
                servers.Add(new HostnamePortPair("ap.spotify.com", 80));
                servers.Add(new HostnamePortPair("ap.spotify.com", 443));
            }
		    /* Try to connect to each server, stop trying when connected. */
		    foreach(HostnamePortPair server in servers)
            {
			    try{
				    /* Try to connect to current server with a timeout of 1 second. */
				    this.client = new TcpClient();
                    this.client.Connect(server.Hostname, server.Port);
                    this.ioStream = client.GetStream();
				    /* Save server for later use. */
				    this.server = server;
                    /*Move the current server to the end of the list*/
                    servers.Remove(server);
                    servers.Add(server);
				    break;
			    }
			    catch(Exception e){
				    System.Console.WriteLine("Error connecting to '" + server + "': " + e.Message);
			    }
		    }

		    /* If connection was not established, return false. */
		    if(!this.client.Client.Connected)
            {
			    throw new ConnectionException("Error connecting to any server!");
		    }

            System.Console.WriteLine("Connected to '" + this.server + "'\n");
        }
        /// <summary>
        /// Disconnect from server
        /// </summary>
        public void Disconnect()
        {
            try
            {
			    /* Close connection to server. */
			    this.client.Close();
                System.Console.WriteLine("Disconnected from '" + this.server + "'\n");
		    }
		    catch(IOException e){
			    throw new ConnectionException("Error disconnecting from '" + this.server + "'!", e);
		    }
        }
      
        public void AddListener(ICommandListener listener)
        {
            this.listeners.Add(listener);
        }
        /// <summary>
        /// Send initial packet (key exchange).
        /// </summary>
	    public void SendInitialPacket()
        {
		    ByteBuffer buffer = ByteBuffer.Allocate(
                2 + 2 + 4 + 4 + 4 + 4 + 4 + 4 + 4 + 16 + 96 + 128 + 1 + 1 + 2 + 0 + this.session.ArrayUsername.Length + 1
		    );

		    /* Append fields to buffer. */
		    buffer.PutShort((short)3); /* Version 3 */
		    buffer.PutShort((short)0); /* Length (update later) */
		    buffer.PutInt(this.session.ClientOs);
            buffer.PutInt(0x00000000); /* Unknown */
            buffer.PutInt(this.session.ClientRevision);
            buffer.PutInt(0x1541ECD0); /* Windows: 0x1541ECD0, Mac OSX: 0x00000000 */
            buffer.PutInt(0x01000000); /* Windows: 0x01000000, Mac OSX: 0x01040000 */
            buffer.PutInt(this.session.ClientId); /* 4 bytes, Windows: 0x010B0029, Mac OSX: 0x026A0200 */
            buffer.PutInt(0x00000001); /* Unknown */
            buffer.Put(this.session.ClientRandom); /* 16 bytes */
            buffer.Put(this.session.DHClientKeyPair.PublicKey.ToByteArray()); /* 96 bytes */
            buffer.Put(this.session.RSAClientKeyPair.PublicKey.ToByteArray()); /* 128 bytes */
            buffer.Put((byte)0); /* Random length */
            buffer.Put((byte)this.session.ArrayUsername.Length); /* Username length */
            buffer.PutShort((short)0x0100); /* Unknown */
		    /* Random bytes here... */
            buffer.Put(this.session.ArrayUsername);
            buffer.Put((byte)0x5F);/* Minor protocol version. */

		    /* Update length byte. */
            buffer.PutShort(2, (short)buffer.Position);
		    buffer.Flip();

		    /* Save initial client packet for auth hmac generation. */
		    this.session.InitialClientPacket = new byte[buffer.Remaining];

		    buffer.Get(this.session.InitialClientPacket);
		    buffer.Flip();

		    /* Send it. */
		    this.Send(buffer);
	    }
        /// <summary>
        /// Receive initial packet (key exchange).
        /// </summary>
        public void ReceiveInitialPacket()
        {
            byte[] buffer = new byte[512];
            int ret, paddingLength, usernameLength;

            /* Save initial server packet for auth hmac generation. 1024 bytes should be enough. */
            ByteBuffer serverPacketBuffer = ByteBuffer.Allocate(1024);

            /* Read server random (first 2 bytes). */
            if ((ret = this.Receive(this.session.ServerRandom, 0, 2)) != 2)
            {
                throw new ProtocolException("Failed to read server random.");
            }

            /* Check if we got a status message. */
            if (this.session.ServerRandom[0] != 0x00)
            {
                /*
                 * Substatuses:
                 * 0x01    : Client upgrade required.
                 * 0x03    : Nonexistent user.
                 * 0x04    : Account has been disabled.
                 * 0x06    : You need to complete your account details.
                 * 0x09    : Your current country doesn't match that set in your profile.
                 * Default : Unknown error
                 */
                StringBuilder message = new StringBuilder(255);

                /* Check substatus and set message. */
                switch (this.session.ServerRandom[1])
                {
                    case 0x01:
                        message.Append("Client upgrade required: ");
                        break;
                    case 0x03:
                        message.Append("Nonexistent user.");
                        break;
                    case 0x04:
                        message.Append("Account has been disabled.");
                        break;
                    case 0x06:
                        message.Append("You need to complete your account details.");
                        break;
                    case 0x09:
                        message.Append("Your current country doesn't match that set in your profile.");
                        break;
                    default:
                        message.Append("Unknown error.");
                        break;
                }

                /* If substatus is 'Client upgrade required', update client revision. */
                if (this.session.ServerRandom[1] == 0x01)
                {
                    if ((ret = this.Receive(buffer, 0x11a)) > 0)
                    {
                        paddingLength = buffer[0x119] & 0xFF;

                        if ((ret = this.Receive(buffer, paddingLength)) > 0)
                        {
                            //string msg = new string(Arrays.copyOfRange(buffer, 0, ret));
                            string msg = Encoding.UTF8.GetString(buffer, 0, ret);
                            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("([0-9]+)\\.([0-9]+)\\.([0-9]+)\\.([0-9]+)\\.[0-9a-z]+");
                            System.Text.RegularExpressions.Match matcher = regex.Match(msg);

                            /* Update client revision. */
                            if (matcher.Success)
                            {
                                /* 
                                 * 0x0266EF51: 40.300.369 -> 0.4.3.369.gd0ec4115
                                 * 0x0266EF5C: 40.300.380 -> 0.4.3.380.g88163066
                                 * 0x0266EF5C: 40.300.383 -> 0.4.3.380.g278a6e51
                                 * 
                                 * major * 1000000000 (???) + minor * 10000000 + maintenance * 100000 + build.
                                 */
                                this.session.ClientRevision = int.Parse(matcher.Groups[2].Value) * 10000000;
                                this.session.ClientRevision += int.Parse(matcher.Groups[3].Value) * 100000;
                                this.session.ClientRevision += int.Parse(matcher.Groups[4].Value);
                            }

                            message.Append(msg);
                        }
                    }
                }

                throw new ProtocolException(message.ToString());
            }

            /* Read server random (next 14 bytes). */
            if ((ret = this.Receive(this.session.ServerRandom, 2, 14)) != 14)
            {
                throw new ProtocolException("Failed to read server random.");
            }

            /* Save server random to packet buffer. */
            serverPacketBuffer.Put(this.session.ServerRandom);

            /* Read server public key (Diffie Hellman key exchange). */
            if ((ret = this.Receive(buffer, 96)) != 96)
            {
                throw new ProtocolException("Failed to read server public key.");
            }

            /* Save DH public key to packet buffer. */
            serverPacketBuffer.Put(buffer, 0, 96);

            /* 
             * Convert key, which is in raw byte form to a DHPublicKey
             * using the DHParameterSpec (for P and G values) of our
             * public key. Y value is taken from raw bytes.
             */
            byte[] aux = new byte[96];
            Array.Copy(buffer, aux, 96);
            this.session.DHServerPublicKey = new DHPublicKey(aux);

            /* Read server blob (256 bytes). */
            if ((ret = this.Receive(this.session.ServerBlob, 0, 256)) != 256)
            {
                throw new ProtocolException("Failed to read server blob.");
            }

            /* Save RSA signature to packet buffer. */
            serverPacketBuffer.Put(this.session.ServerBlob);

            /* Read salt (10 bytes). */
            if ((ret = this.Receive(this.session.Salt, 0, 10)) != 10)
            {
                throw new ProtocolException("Failed to read salt.");
            }

            /* Save salt to packet buffer. */
            serverPacketBuffer.Put(this.session.Salt);

            /* Read padding length (1 byte). */
            if ((paddingLength = this.Receive()) == -1)
            {
                throw new ProtocolException("Failed to read paddling length.");
            }

            /* Save padding length to packet buffer. */
            serverPacketBuffer.Put((byte)paddingLength);

            /* Check if padding length is valid. */
            if (paddingLength <= 0)
            {
                throw new ProtocolException("Padding length is negative or zero.");
            }

            /* Read username length. */
            if ((usernameLength = this.Receive()) == -1)
            {
                throw new ProtocolException("Failed to read username length.");
            }

            /* Save username length to packet buffer. */
            serverPacketBuffer.Put((byte)usernameLength);

            /* Read lengths of puzzle challenge and unknown fields */
            this.Receive(buffer, 8);

            /* Save bytes to packet buffer. */
            serverPacketBuffer.Put(buffer, 0, 8);

            /* Get lengths of puzzle challenge and unknown fields.  */
            ByteBuffer dataBuffer = ByteBuffer.Wrap(buffer, 0, 8);
            int puzzleChallengeLength = dataBuffer.GetShort();
            int unknownLength1 = dataBuffer.GetShort();
            int unknownLength2 = dataBuffer.GetShort();
            int unknownLength3 = dataBuffer.GetShort();

            /* Read padding. */
            if ((ret = this.Receive(buffer, paddingLength)) != paddingLength)
            {
                throw new ProtocolException("Failed to read padding.");
            }

            /* Save padding (random bytes) to packet buffer. */
            serverPacketBuffer.Put(buffer, 0, paddingLength);

            /* Read username into buffer and copy it to 'session.username'. */
            if ((ret = this.Receive(buffer, usernameLength)) != usernameLength)
            {
                throw new ProtocolException("Failed to read username.");
            }

            /* Save username to packet buffer. */
            serverPacketBuffer.Put(buffer, 0, usernameLength);
           
            /* Save username to session. */
             aux = new byte[usernameLength];
            Array.Copy(buffer, aux, usernameLength);
            this.session.ArrayUsername = aux;

            /* Receive puzzle challenge and unknown bytes (more puzzle lengths, seem to be always zero). */
            this.Receive(buffer, 0, puzzleChallengeLength);
            this.Receive(buffer, puzzleChallengeLength, unknownLength1);
            this.Receive(buffer, puzzleChallengeLength + unknownLength1, unknownLength2);
            this.Receive(buffer, puzzleChallengeLength + unknownLength1 + unknownLength2, unknownLength3);

            /* Save to packet buffer. */
            serverPacketBuffer.Put(buffer, 0, puzzleChallengeLength + unknownLength1 + unknownLength2 + unknownLength3);
            serverPacketBuffer.Flip();

            /* Write data from packet buffer to byte array. */
            this.session.InitialServerPacket = new byte[serverPacketBuffer.Remaining];

            serverPacketBuffer.Get(this.session.InitialServerPacket);

            /* Wrap buffer in order to get values. */
            dataBuffer = ByteBuffer.Wrap(buffer, 0, puzzleChallengeLength + unknownLength1 + unknownLength2 + unknownLength3);

            /* Get puzzle denominator and magic. */
            if (dataBuffer.Get() == 0x01)
            { /* 0x01: SHA-1 puzzle, 0x00: no puzzle. */
                this.session.PuzzleDenominator = dataBuffer.Get();
                this.session.PuzzleMagic = dataBuffer.GetInt();
            }
            else
            {
                throw new ProtocolException("Unexpected puzzle challenge.");
            }
        }
        /// <summary>
        /// Send authentication packet (puzzle solution, HMAC).
        /// </summary>
        public void SendAuthenticationPacket()
        {
            ByteBuffer buffer = ByteBuffer.Allocate(20 + 1 + 1 + 4 + 2 + 15 + this.session.PuzzleSolution.Length);

            /* Append fields to buffer. */
            buffer.Put(this.session.AuthHmac); /* 20 bytes */
            buffer.Put((byte)0); /* Random data length */
            buffer.Put((byte)0); /* Unknown. */
            buffer.PutShort((short)this.session.PuzzleSolution.Length);
            buffer.PutInt(0x0000000); /* Unknown. */
            /* Random bytes here... */
            buffer.Put(this.session.PuzzleSolution); /* 8 bytes */
            buffer.Flip();

            /* Send it. */
            this.Send(buffer);
        }
        /// <summary>
        /// Receive authentication packet (status).
        /// </summary>
        public void ReceiveAuthenticationPacket()
        {
            byte[] buffer = new byte[512];
            int payloadLength;

            /* Read status and length. */
            if (this.Receive(buffer, 2) != 2)
            {
                throw new ProtocolException("Failed to read status and length bytes.");
            }

            /* Check status. */
            if (buffer[0] != 0x00)
            {
                throw new ProtocolException("Authentication failed!");
            }

            /* Check payload length. AND with 0x00FF so we don't get a negative integer. */
            if ((payloadLength = buffer[1] & 0xFF) <= 0)
            {
                throw new ProtocolException("Payload length is negative or zero.");
            }

            /* Read payload. */
            if (this.Receive(buffer, payloadLength) != payloadLength)
            {
                throw new ProtocolException("Failed to read payload.");
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SendPacket(int command, ByteBuffer payload)
        {
            ByteBuffer buffer = ByteBuffer.Allocate(1 + 2 + payload.Remaining);

            /* Set IV. */
            this.session.ShannonSend.nonce(IntUtils.ToBytes(this.session.KeySendIv));

            /* Build packet. */
            buffer.Put((byte)command);
            buffer.PutShort((short)payload.Remaining);
            buffer.Put(payload.ToArray());

            byte[] bytes = buffer.ToArray();
            byte[] mac = new byte[4];

            /* Encrypt packet and get MAC. */
            this.session.ShannonSend.encrypt(bytes);
            this.session.ShannonSend.finish(mac);

            buffer = ByteBuffer.Allocate(buffer.Position + 4);
            buffer.Put(bytes);
            buffer.Put(mac);
            buffer.Flip();

            /* Send encrypted packet. */
            this.Send(buffer);

            /* Increment IV. */
            this.session.KeySendIv++;
        }
        /// <summary>
        /// Send a command without payload.
        /// </summary>
        /// <param name="command"></param>
        public void SendPacket(int command)
        {
            this.SendPacket(command, ByteBuffer.Allocate(0));
        }

        private AsyncCallback socketCallBack;
        /// <summary>
        /// Receive a packet (will be decrypted with stream cipher).
        /// </summary>
        public void ReceivePacket()
        {
            if (socketCallBack == null)
                socketCallBack = new AsyncCallback(OnReceivePacketHeaderReceived);

            try
            {
                byte[] header = new byte[3];
                ioStream.BeginRead(header, 0, header.Length, socketCallBack, header);
            }
            catch (SocketException)
            {
                throw new ProtocolException("Failed to read header.");
            }
            //byte[] header = new byte[3];
            //int command, payloadLength, headerLength = 3, macLength = 4;

            ///* Read header. */
            //if (this.Receive(header, headerLength) != headerLength)
            //{
            //    throw new ProtocolException("Failed to read header.");
            //}
        }
        protected virtual void OnReceivePacketHeaderReceived(IAsyncResult asyn)
        {
            byte[] header = (byte[])asyn.AsyncState;
            int command, payloadLength, macLength = 4;

            // TODO: Find a good way to implement detection of closed socket here,
            // so that the function doesn't extrapolate incorrect data from null data.

		    /* Set IV. */
		    this.session.ShannonRecv.nonce(IntUtils.ToBytes(this.session.KeyRecvIv));

		    /* Decrypt header. */
		    this.session.ShannonRecv.decrypt(header);

		    /* Get command and payload length from header. */
		    ByteBuffer headerBuffer = ByteBuffer.Wrap(header);

		    command       = headerBuffer.Get()      & 0xff;
		    payloadLength = headerBuffer.GetShort() & 0xffff;

		    /* Allocate buffer. Account for MAC. */
		    byte[]     bytes  = new byte[payloadLength + macLength];
		    //ByteBuffer buffer = ByteBuffer.Wrap(bytes);

		    /* Limit buffer to payload length, so we can read the payload. */
		    //buffer.Limit = payloadLength;

		    try{
			    
                this.Receive(bytes, 0, payloadLength);
                ////for(int n = payloadLength, r; n > 0 && (r = this.channel.Client.Receive(buffer)) > 0; n -= r);
                //int n = 0;
                //int r = -1;
                //while ((n < payloadLength) && (r != 0))
                //{
                //    r = this.client.Client.Receive(bytes, n, payloadLength - n, SocketFlags.None);
                //    n += r;
                //}
		    }
		    catch(IOException e)
            {
			    throw new ProtocolException("Failed to read payload!", e);
		    }

		    /* Extend it again to payload and mac length. */
		    //buffer.Limit = payloadLength + macLength;

		    try
            {
                this.Receive(bytes, payloadLength, macLength);
                ////for(int n = macLength, r; n > 0 && (r = this.channel.Read(buffer)) > 0; n -= r);
                //int n = payloadLength;
                //int r = -1;
                //while ((n < bytes.Length) && (r != 0))
                //{
                //    r = this.Receive(bytes, n, bytes.Length - n);
                //    n += r;
                //}
		    }
		    catch(IOException e)
            {
			    throw new ProtocolException("Failed to read MAC!", e);
		    }

		    /* Decrypt payload. */
		    this.session.ShannonRecv.decrypt(bytes);
            
            /*Allocate buffer*/
            ByteBuffer buffer = ByteBuffer.Wrap(bytes);

		    /* Get payload bytes from buffer (throw away MAC). */
		    byte[] payload = new byte[payloadLength];

		    buffer.Flip();
		    buffer.Get(payload);

		    /* Increment IV. */
		    this.session.KeyRecvIv++;

		    /* Fire events. */
		    foreach(ICommandListener listener in this.listeners)
            {
			    listener.CommandReceived(command, payload);
		    }
            ReceivePacket();
        }
        /// <summary>
        /// Send cache hash.
        /// </summary>
        public void SendCacheHash()
        {
            ByteBuffer buffer = ByteBuffer.Allocate(20);

            buffer.Put(this.session.CacheHash);
            buffer.Flip();

            this.SendPacket(Command.COMMAND_CACHEHASH, buffer);
        }
        /// <summary>
        /// Request ads. The response is GZIP compressed XML.
        /// </summary>
        public void SendAdRequest(IChannelListener listener, int type)
        {
            /* Create channel and buffer. */
            Channel.Channel channel = new Channel.Channel("Ad-Channel", Channel.ChannelType.TYPE_AD, listener);
            ByteBuffer buffer = ByteBuffer.Allocate(2 + 1);

            /* Append channel id and ad type. */
            buffer.PutShort((short)channel.Id);
            buffer.Put((byte)type); /* 0: audio, 1: banner, 2: fullscreen-banner, 3: unknown.  */
            buffer.Flip();

            /* Register channel. */
            Channel.Channel.Register(channel);

            /* Send packet. */
            this.SendPacket(Command.COMMAND_REQUESTAD, buffer);
        }
        /// <summary>
        /// Get a toplist. The response comes as GZIP compressed XML.
        /// </summary>
        public void SendToplistRequest(IChannelListener listener, Dictionary<string, string> paramargs)
        {
            /* Check if type parameter is present. */
		    if(!paramargs.ContainsKey("type"))
            {
			    throw new ArgumentException("Parameter 'type' not given!");
		    }

		    /* Create a map of parameters and calculate their length. */
		    Dictionary<byte[], byte[]> parameters = new Dictionary<byte[], byte[]>();
		    int parametersLength = 0;

		    foreach(KeyValuePair<String, String> param in paramargs)
            {
			    if(param.Key == null || param.Value == null)
                {
				    continue;
			    }

			    byte[] key   = Encoding.UTF8.GetBytes(param.Key);
			    byte[] value = Encoding.UTF8.GetBytes(param.Value);

			    parametersLength += 1 + 2 + key.Length + value.Length;

			    parameters.Add(key, value);
		    }

		    /* Create channel and buffer. */
		    Channel.Channel    channel = new Channel.Channel("Toplist-Channel", Channel.ChannelType.TYPE_TOPLIST, listener);
		    ByteBuffer buffer  = ByteBuffer.Allocate(2 + 2 + 2 + parametersLength);

		    /* Append channel id, some values, query length and query. */
		    buffer.PutShort((short)channel.Id);
		    buffer.PutInt(0x00000000);

		    foreach(KeyValuePair<byte[], byte[]> parameter in parameters)
            {
			    byte[] key   = parameter.Key;
			    byte[] value = parameter.Value;

			    buffer.Put((byte)key.Length);
			    buffer.PutShort((short)value.Length);
			    buffer.Put(key);
			    buffer.Put(value);
		    }

		    buffer.Flip();

		    /* Register channel. */
            Channel.Channel.Register(channel);

		    /* Send packet. */
		    this.SendPacket(Command.COMMAND_GETTOPLIST, buffer);
        }
        /// <summary>
        /// Request image using a 20 byte id. The response is a JPG.
        /// </summary>
        public void SendImageRequest(IChannelListener listener, string id)
        {
            /* Create channel and buffer. */
            Channel.Channel channel = new Channel.Channel("Image-Channel", Channel.ChannelType.TYPE_IMAGE, listener);
            ByteBuffer buffer = ByteBuffer.Allocate(2 + 2 + 20);

            /* Check length of id. */
            if (id.Length != 40)
            {
                throw new ArgumentException("Image id needs to have a length of 40.");
            }

            /* Append channel id and image hash. */
            buffer.PutShort((short)channel.Id);
            buffer.PutShort((short)0x0000);
            buffer.Put(Hex.ToBytes(id));
            buffer.Flip();

            /* Register channel. */
            Channel.Channel.Register(channel);

            /* Send packet. */
            this.SendPacket(Command.COMMAND_IMAGE, buffer);
        }
        /// <summary>
        /// Search music. The response comes as GZIP compressed XML.
        /// </summary>
        public void SendSearchQuery(IChannelListener listener, string query, int offset, int limit)
        {
            /* Create channel and buffer. */
            byte[] queryBytes = Encoding.UTF8.GetBytes(query);
            Channel.Channel channel = new Channel.Channel("Search-Channel", Channel.ChannelType.TYPE_SEARCH, listener);
            ByteBuffer buffer = ByteBuffer.Allocate(2 + 2 + 6 * 4 + 2 + 1 + queryBytes.Length);

            /* Check offset and limit. */
            if (offset < 0)
            {
                throw new ArgumentException("Offset needs to be >= 0");
            }
            else if ((limit < 0 && limit != -1) || limit == 0)
            {
                throw new ArgumentException("Limit needs to be either -1 for no limit or > 0");
            }

            /* Append channel id, some unknown values, query length and query. */
            buffer.PutShort((short)channel.Id);
            buffer.PutShort((short)0x0000); /* Unknown. */
            buffer.PutInt(offset); /* Track offset. */
            buffer.PutInt(limit); /* Track limit. */
            buffer.PutInt(offset); /* Album offset. */
            buffer.PutInt(limit); /* Album limit. */
            buffer.PutInt(offset); /* Artist offset. */
            buffer.PutInt(limit); /* Artist limit. */
            buffer.PutShort((short)0x0000); /* Unknown. */
            buffer.Put((byte)queryBytes.Length);
            buffer.Put(queryBytes);
            buffer.Flip();

            /* Register channel. */
            Channel.Channel.Register(channel);

            /* Send packet. */
            this.SendPacket(Command.COMMAND_SEARCH, buffer);
        }
        /// <summary>
        /// Search music. The response comes as GZIP compressed XML.
        /// </summary>
        public void SendSearchQuery(IChannelListener listener, string query)
        {
            this.SendSearchQuery(listener, query, 0, -1);
        }
        /// <summary>
        /// Request AES key for a track.
        /// </summary>
        public void SendAesKeyRequest(IChannelListener listener, Track track, Sharpotify.Media.File file)
        {
            /* Create channel and buffer. */
            Channel.Channel channel = new Channel.Channel("AES-Key-Channel", Channel.ChannelType.TYPE_AESKEY, listener);
            ByteBuffer buffer = ByteBuffer.Allocate(20 + 16 + 2 + 2 + 2);

            /* Request the AES key for this file by sending the file id and track id. */
            buffer.Put(Hex.ToBytes(file.Id)); /* 20 bytes */
            buffer.Put(Hex.ToBytes(track.Id)); /* 16 bytes */
            buffer.PutShort((short)0x0000);
            buffer.PutShort((short)channel.Id);
            buffer.PutShort((short)0x0000);
            buffer.Flip();

            /* Register channel. */
            Channel.Channel.Register(channel);

            /* Send packet. */
            this.SendPacket(Command.COMMAND_REQKEY, buffer);
        }
        /// <summary>
        /// Notify server we're going to play.
        /// </summary>
        public void SendPlayRequest()
        {
            /* 
             * Notify the server about our intention to play music, there by allowing
             * it to request other players on the same account to pause.
             * 
             * Yet another client side restriction to annony those who share their
             * Spotify account with not yet invited friends. And as a bonus it won't
             * play commercials and waste bandwidth in vain.
             */
            this.SendPacket(Command.COMMAND_REQUESTPLAY);
        }
        /// <summary>
        /// Request a part of the encrypted file from the server.
        /// 
        /// The data should be decrypted using AES key in CTR mode
        /// with AES key provided and a static IV, incremented for
        /// each 16 byte data processed.
        /// </summary>
        public void SendSubstreamRequest(IChannelListener listener, Sharpotify.Media.File file, int offset, int length)
        {
            /* Create channel and buffer. */
            Channel.Channel channel = new Channel.Channel("Substream-Channel", Channel.ChannelType.TYPE_SUBSTREAM, listener);
            ByteBuffer buffer = ByteBuffer.Allocate(2 + 2 + 2 + 2 + 2 + 2 + 2 + 4 + 20 + 4 + 4);

            /* Append channel id. */
            buffer.PutShort((short)channel.Id);

            /* Unknown 10 bytes. */
            buffer.PutShort((short)0x0800);
            buffer.PutShort((short)0x0000);
            buffer.PutShort((short)0x0000);
            buffer.PutShort((short)0x0000);
            buffer.PutShort((short)0x0000);
            buffer.PutShort((short)0x4e20);

            /* Unknown (static value) */
            buffer.PutInt(200 * 1000);

            /* 20 bytes file id. */
            buffer.Put(Hex.ToBytes(file.Id));

            if (offset % 4096 != 0 || length % 4096 != 0 || length == 0)
            {
                throw new ArgumentException("Offset and length need to be a multiple of 4096.");
            }

            offset >>= 2;
            length >>= 2;

            /* Append offset and length. */
            buffer.PutInt(offset);
            buffer.PutInt(offset + length);
            buffer.Flip();

            /* Register channel. */
            Channel.Channel.Register(channel);

            /* Send packet. */
            this.SendPacket(Command.COMMAND_GETSUBSTREAM, buffer);
        }
        /// <summary>
        /// TODO: untested.
        /// </summary>
        public void SendChannelAbort(int id)
        {
            /* Create channel and buffer. */
            ByteBuffer buffer = ByteBuffer.Allocate(2);

            /* Append channel id. */
            buffer.PutShort((short)id);

            /* Send packet. */
            this.SendPacket(Command.COMMAND_CHANNELABRT, buffer);
        }
        /// <summary>
        /// Get metadata for an artist (type = 1), album (type = 2) or a
        /// list of tracks (type = 3). The response comes as compressed XML.
        /// </summary>
        public void SendBrowseRequest(IChannelListener listener, int type, List<String> ids)
        { 
            /* Create channel and buffer. */
		    Channel.Channel channel = new Channel.Channel("Browse-Channel", Channel.ChannelType.TYPE_BROWSE, listener);
            ByteBuffer buffer = ByteBuffer.Allocate(2 + 2 + 1 + ids.Count * 16 + ((type == 1 || type == 2) ? 4 : 0));

		    /* Check arguments. */
		    if(type != 1 && type != 2 && type != 3){
			    throw new ArgumentException("Type needs to be 1, 2 or 3.");
		    }
		    else if((type == 1 && type == 2) && ids.Count != 1){
			    throw new ArgumentException("Types 1 and 2 only accept a single id.");
		    }

		    /* Append channel id and type. */
		    buffer.PutShort((short)channel.Id);
		    buffer.PutShort((short)0x0000); /* Unknown. */
		    buffer.Put((byte)type);

		    /* Append (16 byte binary, 32 byte hex string) ids. */
		    foreach(string id in ids){
			    /* Check length of id. */
			    if(id.Length != 32)
                {
				    throw new ArgumentException("Id needs to have a length of 32.");
			    }

			    buffer.Put(Hex.ToBytes(id));
		    }

		    /* Append zero. */
		    if(type == 1 || type == 2){
			    buffer.PutInt(0); /* Timestamp of cached version? */
		    }

		    buffer.Flip();

		    /* Register channel. */
            Channel.Channel.Register(channel);

		    /* Send packet. */
		    this.SendPacket(Command.COMMAND_BROWSE, buffer);
        }
        /// <summary>
        /// Browse with only one id.
        /// </summary>
        public void SendBrowseRequest(IChannelListener listener, int type, String id)
        {
            List<String> list = new List<String>();
            list.Add(id);
            this.SendBrowseRequest(listener, type, list);
        }
        /// <summary>
        /// Request replacements for a list of tracks. The response comes as compressed XML.
        /// </summary>
        public void SendReplacementRequest(IChannelListener listener, List<Track> tracks)
        { 
            /* Calculate data length. */
		    int dataLength = 0;

		    foreach(Track track in tracks)
            {
			    if(track.Artist != null && track.Artist.Name != null)
                {
				    dataLength += Encoding.UTF8.GetBytes(track.Artist.Name).Length;
			    }

                if (track.Album != null && track.Album.Name != null)
                {
                    dataLength += Encoding.UTF8.GetBytes(track.Album.Name).Length;
			    }

			    if(track.Title != null)
                {
				    dataLength += Encoding.UTF8.GetBytes(track.Title).Length;
			    }

			    if(track.Length != -1)
                {
                    dataLength += Encoding.UTF8.GetBytes((track.Length / 1000).ToString()).Length;
			    }

			    dataLength += 4; /* Separators */
		    }

		    /* Create channel and buffer. */
		    Channel.Channel channel = new Channel.Channel("Browse-Channel", Channel.ChannelType.TYPE_BROWSE, listener);
		    ByteBuffer buffer  = ByteBuffer.Allocate(2 + 2 + 1 + dataLength);

		    /* Append channel id and type. */
		    buffer.PutShort((short)channel.Id);
		    buffer.PutShort((short)0x0000); /* Unknown. */
		    buffer.Put((byte)0x06);

		    /* Append track info. */
		    foreach(Track track in tracks)
            {
			    if(track.Artist != null && track.Artist.Name != null)
                {
				    buffer.Put(Encoding.UTF8.GetBytes(track.Artist.Name));
			    }

			    buffer.Put((byte)0x01); /* Separator. */

			    if(track.Album != null && track.Album.Name != null)
                {
				    buffer.Put(Encoding.UTF8.GetBytes(track.Album.Name));
			    }

			    buffer.Put((byte)0x01); /* Separator. */

			    if(track.Title != null)
                {
				    buffer.Put(Encoding.UTF8.GetBytes(track.Title));
			    }

			    buffer.Put((byte)0x01); /* Separator. */

			    if(track.Length != -1)
                {
				    buffer.Put(Encoding.UTF8.GetBytes((track.Length / 1000).ToString()));
			    }

			    buffer.Put((byte)0x00); /* Separator. */
		    }

		    buffer.Flip();

		    /* Register channel. */
		    Channel.Channel.Register(channel);

		    /* Send packet. */
		    this.SendPacket(Command.COMMAND_BROWSE, buffer);
        }
        /// <summary>
        /// Request playlist details. The response comes as plain XML.
        /// </summary>
        public void SendPlaylistRequest(IChannelListener listener, string id)
        {
            /* Create channel and buffer. */
            Channel.Channel channel = new Channel.Channel("Playlist-Channel", Channel.ChannelType.TYPE_PLAYLIST, listener);
            ByteBuffer buffer = ByteBuffer.Allocate(2 + 16 + 1 + 4 + 4 + 4 + 1);

            /* Check length of id. */
            if (id != null && id.Length != 32)
            {
                throw new ArgumentException("Playlist id needs to have a length of 32.");
            }

            /* Append channel id, playlist id and some bytes... */
            buffer.PutShort((short)channel.Id);

            /* Playlist container. */
            if (id == null)
            {
                buffer.Put(Hex.ToBytes("00000000000000000000000000000000")); /* 16 bytes */
                buffer.Put((byte)0x00); /* Playlist container identifier. */
            }
            /* Normal playlist. */
            else
            {
                buffer.Put(Hex.ToBytes(id)); /* 16 bytes */
                buffer.Put((byte)0x02); /* Playlist identifier. */
            }
            /*
             * TODO: Other playlist identifiers (e.g. 0x03, starred tracks? inbox?).
             */

            /* TODO: Use those fields to request only the information needed. */
            buffer.PutInt(-1); /* Revision. -1: no cached data. */
            buffer.PutInt(0); /* Number of entries. */
            buffer.PutInt(1); /* Checksum. */
            buffer.Put((byte)0x00); /* Collaborative. */
            buffer.Flip();

            /* Register channel. */
            Channel.Channel.Register(channel);

            /* Send packet. */
            this.SendPacket(Command.COMMAND_GETPLAYLIST, buffer);
        }
        /// <summary>
        /// Change playlist container. The response comes as plain XML.
        /// </summary>
        public void SendChangePlaylistContainer(IChannelListener listener, PlaylistContainer playlistContainer, string xml)
        {
            /* Create channel and buffer. */
            Channel.Channel channel = new Channel.Channel("Change-Playlist-Container-Channel", Channel.ChannelType.TYPE_PLAYLIST, listener);
            byte[] bytes = Encoding.UTF8.GetBytes(xml);
            ByteBuffer buffer = ByteBuffer.Allocate(2 + 16 + 1 + 4 + 4 + 4 + 1 + 1 + bytes.Length);

            /* Append channel id, playlist id and some bytes... */
            buffer.PutShort((short)channel.Id);
            buffer.Put(Hex.ToBytes("00000000000000000000000000000000")); /* 16 bytes */
            buffer.Put((byte)0x00); /* Playlists identifier. */
            buffer.PutInt((int)playlistContainer.Revision);
            buffer.PutInt(playlistContainer.Playlists.Count);
            buffer.PutInt((int)playlistContainer.Checksum);
            buffer.Put((byte)0x00); /* Collaborative */
            buffer.Put((byte)0x03); /* Unknown */
            buffer.Put(bytes);
            buffer.Flip();

            /* Register channel. */
            Channel.Channel.Register(channel);

            /* Send packet. */
            this.SendPacket(Command.COMMAND_CHANGEPLAYLIST, buffer);
        }
        /// <summary>
        /// Change playlist. The response comes as plain XML.
        /// </summary>
        public void SendChangePlaylist(IChannelListener listener, Playlist playlist, string xml)
        {
            /* Create channel and buffer. */
            Channel.Channel channel = new Channel.Channel("Change-Playlist-Channel", Channel.ChannelType.TYPE_PLAYLIST, listener);
            byte[] bytes = Encoding.UTF8.GetBytes(xml);
            ByteBuffer buffer = ByteBuffer.Allocate(2 + 16 + 1 + 4 + 4 + 4 + 1 + 1 + bytes.Length);

            /* Append channel id, playlist id and some bytes... */
            buffer.PutShort((short)channel.Id);
            buffer.Put(Hex.ToBytes(playlist.Id)); /* 16 bytes */
            buffer.Put((byte)0x02); /* Playlist identifier. */
            buffer.PutInt((int)playlist.Revision);
            buffer.PutInt(playlist.Tracks.Count);
            buffer.PutInt((int)playlist.Checksum);
            buffer.Put((byte)(playlist.IsCollaborative ? 0x01 : 0x00));
            buffer.Put((byte)0x03); /* Unknown */
            buffer.Put(bytes);
            buffer.Flip();

            /* Register channel. */
            Channel.Channel.Register(channel);

            /* Send packet. */
            this.SendPacket(Command.COMMAND_CHANGEPLAYLIST, buffer);
        }
        /// <summary>
        /// Create playlist. The response comes as plain XML.
        /// </summary>
        public void SendCreatePlaylist(IChannelListener listener, Playlist playlist, string xml)
        {
            /* Create channel and buffer. */
            Channel.Channel channel = new Channel.Channel("Change-Playlist-Channel", Channel.ChannelType.TYPE_PLAYLIST, listener);
            byte[] bytes = Encoding.UTF8.GetBytes(xml);
            ByteBuffer buffer = ByteBuffer.Allocate(2 + 16 + 1 + 4 + 4 + 4 + 1 + 1 + bytes.Length);

            /* Append channel id, playlist id and some bytes... */
            buffer.PutShort((short)channel.Id);
            buffer.Put(Hex.ToBytes(playlist.Id)); /* 16 bytes */
            buffer.Put((byte)0x02); /* Playlist identifier. */
            buffer.PutInt(0);
            buffer.PutInt(0);
            buffer.PutInt(-1); /* -1: Create playlist. */
            buffer.Put((byte)(playlist.IsCollaborative ? 0x01 : 0x00));
            buffer.Put((byte)0x03); /* Unknown */
            buffer.Put(bytes);
            buffer.Flip();

            /* Register channel. */
            Channel.Channel.Register(channel);

            /* Send packet. */
            this.SendPacket(Command.COMMAND_CHANGEPLAYLIST, buffer);
        }
        /// <summary>
        /// Ping reply (pong).
        /// </summary>
        public void SendPong()
        {
            ByteBuffer buffer = ByteBuffer.Allocate(4);

            /* TODO: Append timestamp? */
            buffer.PutInt(0x00000000);
            buffer.Flip();

            /* Send packet. */
            this.SendPacket(Command.COMMAND_PONG, buffer);
        }
        /// <summary>
        /// Send bytes.
        /// </summary>
        private void Send(ByteBuffer buffer)
        {
		    try
            {
			    //this.client.Client.Send(buffer.ToArray());
                this.ioStream.Write(buffer.ToArray(), 0, buffer.Limit);
		    }
		    catch (IOException e)
            {
			    throw new ProtocolException("Error writing data to socket!", e);
		    }
	    }
        /// <summary>
        /// Receive a single byte.
        /// </summary>
        private int Receive()
        {
            //byte[] buffer = new byte[1];
            int aux = -1;
            try
            {
                //this.client.Client.Receive(buffer);
                //return buffer[0] & 0xff;
                do
                {
                    aux = this.ioStream.ReadByte();
                } while (aux < 0);
                return (aux & 0xff);
            }
            catch (IOException e)
            {
                throw new ProtocolException("Error reading data from socket!", e);
            }
        }
        /// <summary>
        /// Receive bytes.
        /// </summary>
        private int Receive(byte[] buffer, int len)
        {
            return this.Receive(buffer, 0, len);
        }
        /// <summary>
        /// 
        /// </summary>
        private int Receive(byte[] bytes, int off, int len)
        {
            int totalRead = 0;
            try
            {
                int read = -1;
                while ((read != 0) && (totalRead < len))
                {
                    //read = this.client.Client.Receive(bytes, off + totalRead, len - totalRead, SocketFlags.None);
                    read = this.ioStream.Read(bytes, off + totalRead, len - totalRead);
                    totalRead += read;
                }
            }
            catch (Exception)
            {
                //throw new ProtocolException("Error reading data from socket.", exception);
            }
            return totalRead;
        }
        #endregion
    }
}
