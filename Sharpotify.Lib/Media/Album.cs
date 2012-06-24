using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharpotify.Util;

namespace Sharpotify.Media
{
    /// <summary>
    /// Holds information about an album.
    /// </summary>
    public class Album : Media
    {
        #region Fields
        /// <summary>
        /// Name of this album.
        /// </summary>
        private string _name;
        /// <summary>
        /// Artist of this album.
        /// </summary>
        private Artist _artist;
        /// <summary>
        /// The identifier for this albums cover image (32-character string).
        /// </summary>
        private string _cover;
        /// <summary>
        /// The type of this album (compilation, album, single).
        /// </summary>
        private string _type;
        /// <summary>
        /// The review of this album.
        /// </summary>
        private string _review;
        /// <summary>
        /// Release year of this album.
        /// </summary>
        private int _year;
        /// <summary>
        /// A List of discs of this album.
        /// </summary>
        private List<Disc> _discs;
        /// <summary>
        /// Similar albums of this album.
        /// </summary>
        private List<Album> _similarAlbums;
        #endregion
        #region Properties
        /// <summary>
        /// Create a link from this album.
        /// </summary>
        public Link Link
        {
            get { return Sharpotify.Media.Link.Create(this); }
        }
        /// <summary>
        /// Get/Set the albums name.
        /// </summary>
        public string Name { get { return this._name; } set { this._name = value; } }
        /// <summary>
        /// Get/Set the albums artist.
        /// </summary>
        public Artist Artist { get { return this._artist; } set { this._artist = value; } }
        /// <summary>
        /// Get/Set the albums type.
        /// </summary>
        public string Type { get { return this._type; } set { this._type = value; } }
        /// <summary>
        /// Get/Set the albums review.
        /// </summary>
        public string Review { get { return this._review; } set { this._review = value; } }
        /// <summary>
        /// Get/Set discs of this album.
        /// </summary>
        public List<Disc> Discs { get { return this._discs; } set { this._discs = value; } }
        /// <summary>
        /// Get/Set similar albums for this album.
        /// </summary>
        public List<Album> SimilarAlbums { get { return this._similarAlbums; } set { this._similarAlbums = value; } }
        /// <summary>
        /// Get/Set the albums release year.
        /// </summary>
        public int Year 
        { 
            get { return this._year; } 
            set
            {
                if (value < 0)
                    throw new ArgumentException("Expecting a positive year.");

                this._year = value;
            }
        }
        /// <summary>
        /// Get/Set the albums cover image identifier.
        /// </summary>
        public string Cover 
        { 
            get { return this._cover; } 
            set
            {
                if (value == null || value.Length != 40 || !Hex.IsHex(value))
                    throw new ArgumentException("Expecting a 40-character hex string.");

                this._cover = value;
            } 
        }
        
        /// <summary>
        /// Get discs for this album.
        /// </summary>
        public List<Track> Tracks
        {
            get
            {
                List<Track> tracks = new List<Track>();
                foreach (Disc disc in this.Discs)
                    tracks.AddRange(disc.Tracks);
		        return tracks;
            }
        }
        #endregion
        #region Factory
        /// <summary>
        /// Creates an empty <see cref="Album"/> object.
        /// </summary>
        public Album()
        {
            this._name = null;
            this._artist = null;
            this._type = null;
            this._review = null;
            this._discs = new List<Disc>();
            this._similarAlbums = new List<Album>();
        }
        /// <summary>
        /// Creates an <see cref="Album"/> object with the specified <code>id</code>.
        /// </summary>
        /// <param name="id">A 32-character hex string or a Spotify URI.</param>
        public Album(string id) : this(id, null, null)
        {
        }
        /// <summary>
        /// Creates an <see cref="Album"/> object with the specified <code>id</code>, <code>name</code> and <code>artist</code>.
        /// </summary>
        /// <param name="id">A 32-character hex string or a Spotify URI.</param>
        /// <param name="name">Name of the album.</param>
        /// <param name="artist">Artist of the album.</param>
        public Album(string id, string name, Artist artist) : base(id)
        {
            this.Name = name;
            this.Artist = artist;
            this.Type = null;
            this.Review = null;
            this.Discs = new List<Disc>();
            this.SimilarAlbums = new List<Album>();
        }
        #endregion
        #region Methods
        /// <summary>
        /// Set discs for this album.
        /// </summary>
        /// <param name="discs">A List of <see cref="Disc"/> objects.</param>
        public void SetTracks(List<Disc> discs)
        {
            this._discs = discs;
        }
        /// <summary>
        /// Determines if an object is equal to this <see cref="Album"/> object.
        /// If both objects are <see cref="Album"/> objects, it will compare their identifiers.
        /// </summary>
        /// <param name="obj">Another object to compare.</param>
        /// <returns>true of the objects are equal, false otherwise.</returns>
        public override bool Equals(Object obj)
        {
            if (obj is Album)
            {
                Album a = obj as Album;
                if (this.Id == a.Id)
                    return true;
                foreach (string rId in this.Redirects)
                {
                    if (rId == a.Id)
                        return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Return the hash code of this <see cref="Album"/> object. This will give the value returned
	    /// by the hashCode method of the identifier string.
        /// </summary>
        /// <returns>The <see cref="Album"/> objects hash code.</returns>
        public override int GetHashCode()
        {
            return (this.Id != null) ? this.Id.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return string.Format("[Album: {0}, {1}]", this.Artist, this.Name);
        }
        #endregion
    }
}
