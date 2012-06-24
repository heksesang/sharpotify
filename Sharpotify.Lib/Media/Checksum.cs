using System;
using Sharpotify.Util;

namespace Sharpotify.Media
{
    /// <summary>
    /// Subclass of Adler32, supplying methods to calculate
    /// a checksum of different media objects.
    /// </summary>
    public class Checksum :Adler32
    {
        /// <summary>
        /// Update the checksum with a <see cref="Playlist"/>.
        /// </summary>
        /// <param name="playlist">A <see cref="Playlist"/> object.</param>
        public void Update(Playlist playlist)
        {
            this.Update(Hex.ToBytes(playlist.Id));
            this.Update((byte)0x02);
        }
        /// <summary>
        /// Update the checksum with a <see cref="Track"/>.
        /// </summary>
        /// <param name="track">A <see cref="Track"/> object.</param>
        public void Update(Track track)
        {
            this.Update(Hex.ToBytes(track.Id));
            this.Update((byte)0x01);
        }
    }
}
