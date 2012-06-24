using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharpotify.Util;

namespace Sharpotify.Media
{
    public class PlaylistContainer : IEnumerable<Playlist>
    {
        #region Constants
        public static readonly PlaylistContainer Empty = new PlaylistContainer();
        #endregion
        #region Fields
        private string _author;
        private List<Playlist> _playlists;
        private long _revision;
        private long _checksum;
        #endregion
        #region properties
        public string Author { get { return this._author; } set { this._author = value; } }
        public List<Playlist> Playlists { get { return this._playlists; } set { this._playlists = value; } }
        public long Revision { get { return this._revision; } set { this._revision = value; } }
        public long Checksum
        {
            get 
            { 
                Checksum checksum = new Checksum(); 
	            foreach(Playlist playlist in this._playlists)
		            checksum.Update(playlist);
		
		        this._checksum = checksum.Value;
		        return this._checksum;
            }
            set { this._checksum = value; }
        }
        #endregion
        #region Factory
        public PlaylistContainer()
        {
            this._author = null;
            this._playlists = new List<Playlist>();
            this._revision = -1;
            this._checksum = -1;
        }
        #endregion
        #region IEnumerable<Playlist> Members

        public IEnumerator<Playlist> GetEnumerator()
        {
            return Playlists.GetEnumerator();
        }

        #endregion
        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Playlists.GetEnumerator();
        }

        #endregion
    }
}
