using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Sharpotify.Protocol.Channel
{
    /*
     * AES CTR not available in C#
     *  See: http://msdn.microsoft.com/en-us/library/system.security.cryptography.aesmanaged.aesmanaged.aspx
     */
    internal class ChannelAudioHandler : IChannelListener
    {
        #region Fields
        private byte[] _iv = new byte[] { 
            (byte)0x72, (byte)0xe0, (byte)0x67, (byte)0xfb,
			(byte)0xdd, (byte)0xcb, (byte)0xcf, (byte)0x77,
			(byte)0xeb, (byte)0xe8, (byte)0xbc, (byte)0x64,
			(byte)0x3f, (byte)0x63, (byte)0x0d, (byte)0x93
        };
        private RijndaelManaged _cipher = new RijndaelManaged();
        private byte[] _key;
        private int _offset = 0; //NOTE: Never used
        private Stream _outputStream;
        #endregion
        #region Properties

        #endregion
        #region Factory

        public ChannelAudioHandler(byte[] key, Stream output)
        {
            _cipher.BlockSize = 128;
            _cipher.KeySize = 128;
            //_cipher.Key = key;
            //_cipher.IV = _iv;
            _key = key;
            _cipher.Mode = CipherMode.ECB; //CTR not available
            _cipher.Padding = PaddingMode.None;

            _outputStream = output;
        }

        #endregion
        #region Methods

        #endregion
        #region ChannelListener Members

        public void ChannelHeader(Channel channel, byte[] header)
        {
            /* Do nothing. */
        }

        public void ChannelData(Channel channel, byte[] data)
        {
            /* Offsets needed for deinterleaving. */
            int off, w, x, y, z;

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
                        ciphertext[block * 1024 + i + j] ^= (byte)(keystream[j] ^ this._iv[j]);
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

            /* Write data to output stream. */
            try
            {
                this._outputStream.Write(ciphertext, 0, ciphertext.Length - 1024);
            }
            catch (Exception)
            {
                /* Just don't care... */
            }
        }

        public void ChannelError(Channel channel)
        {
            /* Do nothing. */
        }

        public void ChannelEnd(Channel channel)
        {
            this._offset += channel.DataLength;
            Channel.Unregister(channel.Id);
        }

        #endregion
    }
}
