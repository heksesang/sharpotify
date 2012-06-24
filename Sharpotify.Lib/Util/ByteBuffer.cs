using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sharpotify.Util
{
    public class ByteBuffer
    {
        #region fields

        private MemoryStream _ms;
        private int _limit = 0;

        #endregion

        #region properties

        public int Remaining
        {
            get
            {
                return Limit - Position;
            }
        }

        public int Position
        {
            get
            {
                if (_ms.Position > int.MaxValue)
                    throw new OverflowException();
                return (int)_ms.Position;
            }
            set
            {
                if (value < Limit)
                    _ms.Position = value;
            }
        }

        public int Limit
        {
            get { return _limit; }
            set { _limit = value; }
        }

        #endregion

        #region methods

        public byte[] ToArray()
        {
            return _ms.ToArray();
        }

        public void Get(byte[] buffer)
        {
            _ms.Read(buffer, 0, buffer.Length);
        }

        public byte Get()
        {
            int b = _ms.ReadByte();
            if (b >= byte.MinValue && b <= byte.MaxValue)
                return (byte)b;
            throw new OverflowException();
        }

        public void Put(byte[] data)
        {
            Put(data, 0, data.Length);
        }

        public void Put(int index, byte[] data)
        {
            Int64 originalPosition = _ms.Position;
            _ms.Position = index;
            Put(data);
            _ms.Position = originalPosition;
        }

        public void Put(byte[] data, int offset, int length)
        {
            _ms.Write(data, offset, length);
            /*using (BinaryWriter bw = new BinaryWriter(_ms))
                bw.Write(data);*/
        }

        public void Put(byte data)
        {
            _ms.WriteByte(data);
        }

        public void PutInt(int data)
        {
            Put(IntUtils.ToBytes(data));
        }
        public void PutInt(uint data)
        {
            Put(IntUtils.ToBytes(data));
        }

        public int GetInt()
        {
            byte[] buffer = new byte[4];
            _ms.Read(buffer, 0, buffer.Length);
            return IntUtils.BytesToInteger(buffer);
        }

        public void PutShort(Int16 data)
        {
            Put(ShortUtilities.ToBytes(data));
        }

        public void PutShort(int index, Int16 data)
        {
            Put(index, ShortUtilities.ToBytes(data));
        }

        public Int16 GetShort()
        {
            byte[] buffer = new byte[2];
            _ms.Read(buffer, 0, buffer.Length);
            return ShortUtilities.BytesToShort(buffer);
        }

        public byte GetByte()
        {
            return (byte)_ms.ReadByte();
        }

        public void Flip()
        {
            this.Limit = (int)_ms.Length;
            this.Position = 0;
            _ms.Position = 0;
        }
        public void Clear()
        {
            this.Limit = this.Position;
            _ms.Position = 0;
        }

        #endregion

        #region construction

        public ByteBuffer(int capacity)
        {
            _ms = new MemoryStream(capacity);
            _limit = capacity;
        }

        public ByteBuffer(byte[] data)
        {
            _ms = new MemoryStream(data);
            _limit = data.Length;
        }

        public ByteBuffer(byte[] data, int offset, int length)
        {
            _ms = new MemoryStream(data, offset, length);
        }

        public static ByteBuffer Allocate(int capacity)
        {
            return new ByteBuffer(capacity);
        }

        public static ByteBuffer Wrap(byte[] data)
        {
            return new ByteBuffer(data);
        }

        public static ByteBuffer Wrap(byte[] data, int offset, int length)
        {
            return new ByteBuffer(data, offset, length);
        }

        #endregion
    }
}
