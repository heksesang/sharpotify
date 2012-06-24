// Adler32.cs - Computes Adler32 data checksum of a data stream
// Copyright (C) 2001 Mike Krueger
//
// This file was translated from java, it was part of the GNU Classpath
// Copyright (C) 1999, 2000, 2001 Free Software Foundation, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
// 
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.

using System;


namespace Sharpotify.Util
{
    public class Adler32
    {
        /// <summary>
        /// largest prime smaller than 65536
        /// </summary>
        readonly static uint BASE = 65521;

        uint checksum;

        /// <summary>
        /// Returns the Adler32 data checksum computed so far.
        /// </summary>
        public long Value
        {
            get
            {
                return checksum;
            }
        }

        /// <summary>
        /// Creates a new instance of the <code>Adler32</code> class.
        /// The checksum starts off with a value of 1.
        /// </summary>
        public Adler32()
        {
            Reset();
        }

        /// <summary>
        /// Resets the Adler32 checksum to the initial value.
        /// </summary>
        public void Reset()
        {
            checksum = 1; //Initialize to 1
        }

        /// <summary>
        /// Updates the checksum with the byte b.
        /// </summary>
        /// <param name="bval">
        /// the data value to add. The high byte of the int is ignored.
        /// </param>
        public void Update(int bval)
        {
            //We could make a length 1 byte array and call update again, but I
            //would rather not have that overhead
            uint s1 = checksum & 0xFFFF;
            uint s2 = checksum >> 16;

            s1 = (s1 + ((uint)bval & 0xFF)) % BASE;
            s2 = (s1 + s2) % BASE;

            checksum = (s2 << 16) + s1;
        }

        /// <summary>
        /// Updates the checksum with the bytes taken from the array.
        /// </summary>
        /// <param name="buffer">
        /// buffer an array of bytes
        /// </param>
        public void Update(byte[] buffer)
        {
            Update(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Updates the checksum with the bytes taken from the array.
        /// </summary>
        /// <param name="buf">
        /// an array of bytes
        /// </param>
        /// <param name="off">
        /// the start of the data used for this update
        /// </param>
        /// <param name="len">
        /// the number of bytes to use for this update
        /// </param>
        public void Update(byte[] buf, int off, int len)
        {
            if (buf == null)
            {
                throw new ArgumentNullException("buf");
            }

            if (off < 0 || len < 0 || off + len > buf.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            //(By Per Bothner)
            uint s1 = checksum & 0xFFFF;
            uint s2 = checksum >> 16;

            while (len > 0)
            {
                // We can defer the modulo operation:
                // s1 maximally grows from 65521 to 65521 + 255 * 3800
                // s2 maximally grows by 3800 * median(s1) = 2090079800 < 2^31
                int n = 3800;
                if (n > len)
                {
                    n = len;
                }
                len -= n;
                while (--n >= 0)
                {
                    s1 = s1 + (uint)(buf[off++] & 0xFF);
                    s2 = s2 + s1;
                }
                s1 %= BASE;
                s2 %= BASE;
            }

            checksum = (s2 << 16) | s1;
        }
    }
}
