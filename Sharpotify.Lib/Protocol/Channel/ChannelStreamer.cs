using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Sharpotify.Cache;
using Sharpotify.Media;
using Sharpotify.Util;
using Sharpotify.Exceptions;

namespace Sharpotify.Protocol.Channel
{
    internal class ChannelStreamer : IChannelListener
    {
        #region Fields

        /* Decryption stuff. */
        private byte[] _iv = new byte[] { 
            (byte)0x72, (byte)0xe0, (byte)0x67, (byte)0xfb,
			(byte)0xdd, (byte)0xcb, (byte)0xcf, (byte)0x77,
			(byte)0xeb, (byte)0xe8, (byte)0xbc, (byte)0x64,
			(byte)0x3f, (byte)0x63, (byte)0x0d, (byte)0x93
        };
        private RijndaelManaged _cipher = new RijndaelManaged();
        private byte[] _key;

        /* Requesting and loading stuff. */
        private Media.File _file;
	    private Protocol _protocol;
	    private int _channelOffset = 0;
	    private int _channelLength;
	    private int _channelTotal = 0;
	    private SpotifyOggHeader _header = null;
	    private MusicStream _output;
    	
	    /* Caching of substreams. */
	    private SubstreamCache _cache = new SubstreamCache();
	    private byte[] _cacheData;

        #endregion

        #region Factory
        public ChannelStreamer(Protocol protocol, Media.File file, byte[] key, MusicStream output)
        {
            if (key.Length != (128 / 8))
            {
                output.AllAvailable = true;
                throw new InvalidDataException("Encryption key for channel must be 128-bit.");
            }

            _cipher.BlockSize = 128;
            _cipher.KeySize = 128;
            _key = key;
            _cipher.Mode = CipherMode.ECB; //CTR not available
            _cipher.Padding = PaddingMode.None;

            _output = output;

            _file = file;
            _protocol = protocol;

            _channelLength = 160 * 1024 * 5 / 8; /* 160 kbit * 5 seconds. */

            /* Send first substream request. */
            string hash = this._cache.Hash(this._file, this._channelOffset, this._channelLength);

            if (this._cache != null && this._cache.Contains("substream", hash))
            {
                this._cache.Load("substream", hash, this);
            }
            else
            {
                try
                {
                    this._protocol.SendSubstreamRequest(this, this._file, this._channelOffset, this._channelLength);
                }
                catch (ProtocolException)
                {
                    return;
                }
            }
        }

        #endregion


        #region ChannelListener Members

        public void ChannelHeader(Channel channel, byte[] header)
        {
            this._cacheData = new byte[this._channelLength];
            this._channelTotal = 0;
        }

        public void ChannelData(Channel channel, byte[] data)
        {
            /* Offsets needed for deinterleaving. */
            int off, w, x, y, z;

            /* Copy data to cache buffer. */
            for (int i = 0; i < data.Length; i++)
            {
                this._cacheData[this._channelTotal + i] = data[i];
            }

            /* Allocate space for ciphertext. */
            byte[] ciphertext = new byte[data.Length + 1024];
            byte[] keystream = new byte[16];

            /* Decrypt each 1024 byte block. */
            for (int block = 0; block < data.Length / 1024; block++)
            {
                /* Deinterleave the 4x256 byte blocks. */
                off = block * 1024;
                w = block * 1024 + 0 * 256;
                x = block * 1024 + 1 * 256;
                y = block * 1024 + 2 * 256;
                z = block * 1024 + 3 * 256;

                for (int i = 0; i < 1024 && (block * 1024 + i) < data.Length; i += 4)
                {
                    ciphertext[off++] = data[w++];
                    ciphertext[off++] = data[x++];
                    ciphertext[off++] = data[y++];
                    ciphertext[off++] = data[z++];
                }

                /* Decrypt 1024 bytes block. This will fail for the last block. */
                for (int i = 0; i < 1024 && (block * 1024 + i) < data.Length; i += 16)
                {
                    /* Produce 16 bytes of keystream from the IV. */
                    try
                    {
                        var crypt = _cipher.CreateEncryptor(_key, _iv);
                        keystream = crypt.TransformFinalBlock(this._iv, 0, this._iv.Length);
                    }
                    catch (Exception)
                    {
                    }

                    /* 
                     * Produce plaintext by XORing ciphertext with keystream.
                     * And somehow I also need to XOR with the IV... Please
                     * somebody tell me what I'm doing wrong, or is it the
                     * Java implementation of AES? At least it works like this.
                     */
                    // FIXME: Does the IV needs to be XORed in C# ?
                    for (int j = 0; j < 16; j++)
                    {
                        //ciphertext[block * 1024 + i + j] ^= (byte)(keystream[j] ^ this._iv[j]);
                        ciphertext[block * 1024 + i + j] ^= keystream[j];
                    }

                    /* Update IV counter. */
                    for (int j = 15; j >= 0; j--)
                    {
                        this._iv[j] += 1;

                        if ((int)(this._iv[j] & 0xFF) != 0)
                        {
                            break;
                        }
                    }

                    /* Set new IV. */
                    this._cipher.IV = this._iv;
                }
            }


            /* Save data to output stream. */
            try
            {
                off = 0;

                /* Check if we decoded the header yet. */
                if (this._header == null)
                {
                    /* Get header from data. */
                    byte[] bytes = new byte[167];
                    Array.Copy(ciphertext, bytes, bytes.Length);

                    /* Decode header. */
                    this._header = new SpotifyOggHeader(bytes);

                    off = 167;

                    //Set stream length
                    this._output.SetLength(this._header.Size);
                }

                this._output.WriteInternal(ciphertext, off, data.Length - off);
                //this._output.Write(ciphertext, off, data.Length - off);
                //this._output.Flush();

                /* 
                 * Don't subtract 'off' here! Otherwise we would
                 * accidentially close the stream in channelEnd!
                 */
                this._channelTotal += data.Length;
            }
            catch (Exception)
            {
                /* Don't care. */
            }
        }

        public void ChannelError(Channel channel)
        {
            /* Ignore */
        }

        public void ChannelEnd(Channel channel)
        {
            /* Create cache hash. */
            string hash = this._cache.Hash(this._file, this._channelOffset, this._channelLength);

            /* Save to cache. */
            if (this._cache != null && !this._cache.Contains("substream", hash))
            {
                this._cache.Store("substream", hash, this._cacheData, this._channelTotal);
            }

            /* Send next substream request. */
            try
            {
                if (this._channelTotal < this._channelLength)
                {
                    this._output.AllAvailable = true;

                    return;
                }

                this._channelOffset += this._channelLength;

                hash = this._cache.Hash(this._file, this._channelOffset, this._channelLength);

                if (this._cache != null && this._cache.Contains("substream", hash))
                {
                    this._cache.Load("substream", hash, this);
                }
                else
                {
                    this._protocol.SendSubstreamRequest(this, this._file, this._channelOffset, this._channelLength);
                }
            }
            catch (Exception)
            {
                /* Ignore. */
            }

            Channel.Unregister(channel.Id);
        }

        #endregion
    }
}
