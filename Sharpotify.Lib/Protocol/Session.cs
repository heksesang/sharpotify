using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Sharpotify.Crypto;
using Sharpotify.Exceptions;
using Sharpotify.Util;

namespace Sharpotify.Protocol
{
    public class Session
    {
        #region Fields
        /* Spotify protocol to send and receive data. */
        private Protocol _protocol;

        /* Client identification */
        protected int _clientId;
        protected int _clientOs;
        protected int _clientRevision;

        /* 16 bytes of Shannon encryption output with random key */
        protected byte[] _clientRandom;
        protected byte[] _serverRandom;

        /* 
        * Blob (1536-bit RSA signature at offset 128)
        * is received at offset 16 in the cmd=0x02 packet.
        */
        protected byte[] _serverBlob;

        /* Username, password, salt, auth hash, auth HMAC and country. */
        protected byte[] _username;
        protected byte[] _password;
        protected byte[] _salt;
        protected byte[] _authHash;
        protected string _country;

        /* DH and RSA keys. */
        protected DHKeyPair _dhClientKeyPair;
        protected DHPublicKey _dhServerPublicKey;
        protected byte[] _dhSharedKey;
        protected RSAKeyPair _rsaClientKeyPair;

        /* 
	     * Output form HMAC SHA-1, used for keying HMAC
	     * and for keying Shannon stream cipher.
	     */
        protected byte[] _keyHmac;
        protected byte[] _authHmac;
        protected byte[] _keyRecv;
        protected byte[] _keySend;
        protected int _keyRecvIv;
        protected int _keySendIv;

        /* Shannon stream cipher */
        protected Shannon _shannonSend;
        protected Shannon _shannonRecv;

        /*
	     * Waste some CPU time while computing a 32-bit value,
	     * that byteswapped and XOR'ed with a magic, modulus
	     * 2^deniminator becomes zero.
	     */
        protected int _puzzleDenominator;
        protected int _puzzleMagic;
        protected byte[] _puzzleSolution;
        /* Cache hash. Automatically generated, but we're lazy. */
        protected byte[] _cacheHash;

        /* Needed for auth hmac. */
        protected byte[] _initialClientPacket;
        protected byte[] _initialServerPacket;

        /* Client operating systems. */
        protected static readonly int CLIENT_OS_WINDOWS_X86 = 0x00000000; /* Windows x86 */
        protected static readonly int CLIENT_OS_MACOSX_X86 = 0x00000100; /* Mac OSX x86 */
        protected static readonly int CLIENT_OS_UNKNOWN_1 = 0x00000200; /* libspotify (guess) */
        protected static readonly int CLIENT_OS_UNKNOWN_2 = 0x00000300; /* iPhone? / Android? / Symbian? */
        protected static readonly int CLIENT_OS_UNKNOWN_3 = 0x00000400; /* iPhone? / Android? / Symbian? */
        protected static readonly int CLIENT_OS_MACOSX_PPC = 0x00000500; /* Mac OSX PPC */
        protected static readonly int CLIENT_OS_UNKNOWN_4 = 0x00000600; /* iPhone? / Android? / Symbian? */

        /* Client ID and revision (Always up to date! ;-P) */
        protected static readonly int CLIENT_ID = 0x01040101; /* 0x010B0029 */
        protected static readonly int CLIENT_REVISION = unchecked((int)0xFFFFFFFF);

        #endregion
        #region Properties
        public byte[] ArrayUsername { get { return this._username; } internal set { this._username = value; } }
        public string StringUsername { get { return Encoding.UTF8.GetString(this._username); } }
        public RSAPublicKey RSAPublicKey { get { return this._rsaClientKeyPair.PublicKey; } }
        public int ClientOs { get { return this._clientOs; } }
        public int ClientRevision { get { return this._clientRevision; } set { this._clientRevision = value; } }
        public int ClientId { get { return this._clientId; } }
        public DHKeyPair DHClientKeyPair { get { return this._dhClientKeyPair; } }
        public DHPublicKey DHServerPublicKey { get { return this._dhServerPublicKey; } internal set { this._dhServerPublicKey = value; } }
        public RSAKeyPair RSAClientKeyPair { get { return this._rsaClientKeyPair; } }
        public byte[] InitialClientPacket { get { return this._initialClientPacket; } internal set { this._initialClientPacket = value; } }
        public byte[] InitialServerPacket { get { return this._initialServerPacket; } internal set { this._initialServerPacket = value; } }
        public byte[] ClientRandom { get { return this._clientRandom; } internal set { this._clientRandom = value; } }
        public byte[] ServerRandom { get { return this._serverRandom; } internal set { this._serverRandom = value; } }
        public byte[] ServerBlob { get { return this._serverBlob; } internal set { this._serverBlob = value; } }
        public byte[] Salt { get { return this._salt; } internal set { this._salt = value; } }
        public int PuzzleDenominator { get { return this._puzzleDenominator; } internal set { this._puzzleDenominator = value; } }
        public int PuzzleMagic { get { return this._puzzleMagic; } internal set { this._puzzleMagic = value; } }
        public byte[] PuzzleSolution { get { return this._puzzleSolution; } internal set { this._puzzleSolution = value; } }
        public byte[] AuthHmac { get { return this._authHmac; } internal set { this._authHmac = value; } }
        public Shannon ShannonSend { get { return this._shannonSend; } }
        public Shannon ShannonRecv { get { return this._shannonRecv; } }
        public int KeySendIv { get { return this._keySendIv; } set { this._keySendIv = value; } }
        public int KeyRecvIv { get { return this._keyRecvIv; } set { this._keyRecvIv = value; } }
        public byte[] CacheHash { get { return this._cacheHash; } internal set { this._cacheHash = value; } }
        #endregion
        #region Factory
        public Session()
            : this(CLIENT_OS_WINDOWS_X86, CLIENT_REVISION)
        {

        }
        public Session(int clientOS, int clientRevision)
        {
            /* Initialize protocol with this session. */
            this._protocol = new Protocol(this);

            /* Set client properties. */
            this._clientId = CLIENT_ID;
            this._clientOs = clientOS;
            this._clientRevision = clientRevision;

            /* Client and server generate 16 random bytes each. */
            this._clientRandom = new byte[16];
            this._serverRandom = new byte[16];

            RandomBytes.GetRandomBytes(ref this._clientRandom);

            /* Allocate buffer for server RSA key. */
            this._serverBlob = new byte[256];

            /* Allocate buffer for salt and auth hash. */
            this._username = null;
            this._password = null;
            this._salt = new byte[10];
            this._authHash = new byte[20];

            /*
             * Create a private and public DH key and allocate buffer
             * for shared key. This, along with key signing, is used
             * to securely agree on a session key for the Shannon stream
             * cipher.
             */
            this._dhClientKeyPair = DH.GenerateKeyPair(768);
            this._dhSharedKey = new byte[96];

            /* Generate RSA key pair. */
            this._rsaClientKeyPair = Sharpotify.Crypto.RSA.GenerateKeyPair(1024);

            /* Allocate buffers for HMAC and Shannon stream cipher keys. */
            this._keyHmac = new byte[20];
            this._authHmac = new byte[20];
            this._keyRecv = new byte[32];
            this._keySend = new byte[32];
            this._keyRecvIv = 0;
            this._keySendIv = 0;

            /* Stream cipher instances. */
            this._shannonRecv = new Shannon();
            this._shannonSend = new Shannon();

            /* Allocate buffer for puzzle solution. */
            this._puzzleDenominator = 0;
            this._puzzleMagic = 0;
            this._puzzleSolution = new byte[8];

            /* Found in Storage.dat (cache) at offset 16. Modify first byte of cache hash. */
            this._cacheHash = new byte[]{
			    (byte)0xf4, (byte)0xc2, (byte)0xaa, (byte)0x05,
			    (byte)0xe8, (byte)0x25, (byte)0xa7, (byte)0xb5,
			    (byte)0xe4, (byte)0xe6, (byte)0x59, (byte)0x0f,
			    (byte)0x3d, (byte)0xd0, (byte)0xbe, (byte)0x0a,
			    (byte)0xef, (byte)0x20, (byte)0x51, (byte)0x95
		    };
            this._cacheHash[0] = (byte)new Random().Next();

            /* Not initialized. */
            this._initialClientPacket = null;
            this._initialServerPacket = null;
        }
        #endregion
        #region Methods
        public Protocol Authenticate(string username, string password)
        {
            /* Number of authentication tries. */
            int tries = 3;

            /* Set username and password. */
            this._username = Encoding.UTF8.GetBytes(username);
            this._password = Encoding.UTF8.GetBytes(password);

            while (true)
            {
                /* Connect to a spotify server. */
                this._protocol.Connect();

                /* Send and receive initial packets. */
                try
                {
                    this._protocol.SendInitialPacket();
                    this._protocol.ReceiveInitialPacket();

                    break;
                }
                catch (ProtocolException e)
                {
                    if (tries-- > 0)
                    {
                        continue;
                    }

                    throw new AuthenticationException(e);
                }
            }

            /* Generate auth hash. */
            this.GenerateAuthHash();

            /* Compute shared key (Diffie Hellman key exchange). */
            this._dhSharedKey = DH.ComputeSharedKey(this._dhClientKeyPair.PrivateKey, this._dhServerPublicKey);

            /* Prepare a message to authenticate. */
            ByteBuffer buffer = ByteBuffer.Allocate(((this._authHash.Length + this._clientRandom.Length) + this._serverRandom.Length) + 1);

            /* Append auth hash, client and server random to message. */
            buffer.Put(this._authHash);
            buffer.Put(this._clientRandom);
            buffer.Put(this._serverRandom);
            buffer.Put((byte)0x00); /* Changed later */
            buffer.Flip();

            /* Get message bytes and allocate space for HMACs. */
            byte[] bytes = new byte[buffer.Remaining];
            byte[] hmac = new byte[5 * 20];
            int offset = 0;

            buffer.Get(bytes);

            /* Run HMAC SHA-1 over message. 5 times. */
            for (int i = 1; i <= 5; i++)
            {
                /* Change last byte (53) of message. */
                bytes[bytes.Length - 1] = (byte)i;

                /* Compute HMAC SHA-1 using the shared key. */
                Hash.HmacSha1(bytes, this._dhSharedKey, hmac, offset);

                /* Overwrite first 20 bytes of message with output from this round. */
                for (int j = 0; j < 20; j++)
                {
                    bytes[j] = hmac[offset + j];
                }

                /* Advance to next position. */
                offset += 20;
            }
            /* Use field of HMACs to setup keys for Shannon stream cipher (key length: 32). */
            Array.Copy(hmac, 20, this._keySend, 0, 32);
            Array.Copy(hmac, 52, this._keyRecv, 0, 32);

            /* Set stream cipher keys. */
            this._shannonSend.key(this._keySend);
            this._shannonRecv.key(this._keyRecv);

            /* 
		     * First 20 bytes of HMAC output is used to key another HMAC computed
		     * for the second authentication packet send by the client.
		     */
            Array.Copy(hmac, 0, this._keyHmac, 0, 20);

            /* Solve puzzle */
            this.SolvePuzzle();

            /* Generate HMAC */
            this.GenerateAuthHmac();
            try
            {
                this._protocol.SendAuthenticationPacket();
                this._protocol.ReceiveAuthenticationPacket();
            }
            catch (ProtocolException e)
            {
                throw new AuthenticationException(e);
            }
            return this._protocol;
        }

        private void GenerateAuthHash()
        {
            ByteBuffer buffer = ByteBuffer.Allocate(this._salt.Length + 1 + this._password.Length);
            buffer.Put(this._salt); /* 10 bytes */
            buffer.Put((byte)' ');
            buffer.Put(this._password);
            this._authHash = Hash.Sha1(buffer.ToArray());
        }

        private void GenerateAuthHmac()
        {
            ByteBuffer buffer = ByteBuffer.Allocate(
                        this._initialClientPacket.Length +
                        this._initialServerPacket.Length
                        + 1 + 1 + 2 + 4 + 0 + this._puzzleSolution.Length);
            buffer.Put(this._initialClientPacket);
            buffer.Put(this._initialServerPacket);
            buffer.Put((byte)0);/* Random data length */
            buffer.Put((byte)0);/* Unknown */
            buffer.PutShort((short)this._puzzleSolution.Length);
            buffer.PutInt(0x0000000);/* Unknown */
            /* Random bytes here... */
            buffer.Put(this._puzzleSolution);/* 8 bytes */

            this._authHmac = Hash.HmacSha1(buffer.ToArray(), this._keyHmac);
        }

        private void SolvePuzzle()
        {
            long denominator, nominatorFromHash;
            byte[] digest;

            ByteBuffer buffer = ByteBuffer.Allocate(
                this._serverRandom.Length + this._puzzleSolution.Length
            );

            /* Modulus operation by a power of two. */
            denominator = 1 << this._puzzleDenominator;
            denominator--;

            /* 
             * Compute a hash over random data until
             * (last dword byteswapped XOR magic number)
             * mod denominator by server produces zero.
             */
            do
            {
                /* Let's waste some precious pseudorandomness. */
                RandomBytes.GetRandomBytes(ref this._puzzleSolution);

                /* Buffer with server random and random bytes (puzzle solution). */
                buffer.Clear();
                buffer.Put(this._serverRandom);
                buffer.Put(this._puzzleSolution);

                /* Calculate digest. */
                digest = Hash.Sha1(buffer.ToArray());

                /* Convert bytes to integer (Java is big-endian). */
                nominatorFromHash = ((digest[16] & 0xFF) << 24) |
                                    ((digest[17] & 0xFF) << 16) |
                                    ((digest[18] & 0xFF) << 8) |
                                    ((digest[19] & 0xFF));

                /* XOR with a fancy magic. */
                nominatorFromHash ^= this._puzzleMagic;
            } while ((nominatorFromHash & denominator) != 0);
        }
        #endregion
    }
}