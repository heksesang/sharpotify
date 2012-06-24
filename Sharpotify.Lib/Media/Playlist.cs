using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharpotify.Util;


namespace Sharpotify.Media
{
    /// <summary>
    /// Holds information about a playlist.
    /// </summary>
    public class Playlist : IEnumerable<Track>
    {
        #region Fields
        private string _id;
        private string _name;
        private string _author;
        private List<Track> _tracks;
        private long _revision;
        private long _checksum;
        private bool _collaborative;
        private string _description;
        private string _picture;
        #endregion
        #region Properties
        public string Id { get { return this._id; } set { this._id = value; } }
        public string Name { get { return this._name; } set { this._name = value; } }
        public string Author { get { return this._author; } set { this._author = value; } }
        public List<Track> Tracks { get { return this._tracks; } set { this._tracks = value; } }
        public bool HasTracks { get { return this._tracks.Count > 0; } }
        public long Revision { get { return this._revision; } set { this._revision = value; } }
        public bool IsCollaborative { get { return this._collaborative; } set { this._collaborative = value; } }
        public string Description { get { return this._description; } set { this._description = value; } }
        public string Picture { get { return this._picture; } set { this._picture = value; } }
        /// <summary>
        /// Get and update/Set the checksum of this playlist.
        /// </summary>
        public long Checksum 
        { 
            get 
            { 
               Checksum checksum = new Checksum(); 

		        foreach(Track track in this._tracks)
			        checksum.Update(track);

		        this._checksum = checksum.Value;

                return this._checksum;
            } 
            set { this._checksum = value; } 
        }
        public string Link
        {
            get
            {
                return string.Format("http://open.spotify.com/user/{0}/playlist/{1}", this.Author, SpotifyURI.ToBase62(this.Id));
            }
        }
        #endregion
        #region Factory
        public Playlist() 
        {
            this._id = null;
            this._name = null;
            this._author = null;
            this._tracks = new List<Track>();
            this._revision = -1;
            this._checksum = -1;
            this._collaborative = false;
            this._description = null;
            this._picture = null;
        }
        public Playlist(string id) : this(id, null, null, false)
        {
            
        }
        public Playlist(string id, string name, string author, bool collaborative)
        {
            /* Check if id is a 32-character hex string. */
            if (id.Length == 32 && Hex.IsHex(id))
            {
                this._id = id;
            }
            /* Otherwise try to parse it as a Spotify URI. */
            else
            {
                try
                {
                    this._id = Sharpotify.Media.Link.Create(id).Id;
                }
                catch (Link.InvalidSpotifyURIException e)
                {
                    throw new ArgumentException(
                        "Given id is neither a 32-character" +
                        "hex string nor a valid Spotify URI.", e
                    );
                }
            }

            /* Set other playlist properties. */
            this._name = name;
            this._author = author;
            this._tracks = new List<Track>();
            this._revision = -1;
            this._checksum = -1;
            this._collaborative = collaborative;
            this._description = null;
            this._picture = null;
        }
        public static Playlist FromResult(string name, string author, Result result)
        {
            Playlist playlist = new Playlist();

            playlist.Name = name;
            playlist.Author = author;

            foreach (Track track in result.Tracks)
                playlist.Tracks.Add(track);

            return playlist;
        }
        #endregion
        #region Methods
        public override bool Equals(Object obj)
        {
            if (obj is Playlist)
            {
                Playlist a = obj as Playlist;
                if (this._id == a.Id)
                    return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (this._id != null) ? this._id.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return string.Format("[Playlist: {0}, {1}]", this.Author, this.Name);
        }
        #endregion
        #region IEnumerable<Track> Members
        public IEnumerator<Track> GetEnumerator()
        {
            return this.Tracks.GetEnumerator();
        }

        #endregion
        #region IEnumerable Members
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.Tracks.GetEnumerator();
        }

        #endregion
    }
}
