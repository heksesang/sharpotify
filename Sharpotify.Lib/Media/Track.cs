using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharpotify.Util;

namespace Sharpotify.Media
{
    /// <summary>
    /// Holds information about a track.
    /// </summary>
    public class Track : Media
    {
        #region Fields
        /// <summary>
        /// Title of this track.
        /// </summary>
        private string _title;
        /// <summary>
        /// <see cref="Artist"/> of this track.
        /// </summary>
        private Artist _artist;
        /// <summary>
        /// <see cref="_album"/> this track belongs to.
        /// </summary>
        private Album _album;
        /// <summary>
        /// Release year of this track.
        /// </summary>
        private int _year = -1;
        /// <summary>
        /// Track number on a certain disk.
        /// </summary>
        private int _trackNumber = -1;
        /// <summary>
        /// Length of this track in seconds.
        /// </summary>
        private int _length = -1;
        /// <summary>
        /// Files available for this track.
        /// </summary>
        private List<File> _files;
        /// <summary>
        /// The identifier for this tracks cover image (32-character string).
        /// </summary>
        private string _cover;
        /// <summary>
        /// Similar tracks of this track.
        /// </summary>
        private List<Track> _similarTracks;
        /// <summary>
        /// If this track is explicit.
        /// </summary>
        private bool _explicit;
        #endregion
        #region Properties
        /// <summary>
        /// Create a link from this track.
        /// </summary>
        public Link Link
        {
            get { return Sharpotify.Media.Link.Create(this); }
        }
        /// <summary>
        /// Get/Set the tracks title.
        /// </summary>
        public string Title { get { return _title; } set { _title = value; } }
        /// <summary>
        /// Get/Set the tracks artist.
        /// </summary>
        public Artist Artist { get { return _artist; } set { _artist = value; } }
        /// <summary>
        /// Get/Set the tracks album.
        /// </summary>
        public Album Album { get { return _album; } set { _album = value; } }
        /// <summary>
        /// Get/Set the list of <see cref="File"/> objects for this track.
        /// </summary>
        public List<File> Files { get { return _files; } set { _files = value; } }
        /// <summary>
        /// Get/Set the tracks cover image identifier.
        /// </summary>
        public string Cover { get { return _cover; } set { _cover = value; } }
        /// <summary>
        /// Get/Set similar tracks for this track.
        /// </summary>
        public List<Track> SimilarTracks { get { return _similarTracks; } set { _similarTracks = value; } }
        /// <summary>
        /// Get/Set the tracks release year.
        /// </summary>
        public int Year
        {
            get { return _year; }
            set
            {
                if (value < 0)
			        throw new ArgumentException("Expecting a positive year.");
                _year = value;
            }
        }
        /// <summary>
        /// Get/Set the tracks number on a certain disk.
        /// </summary>
        public int TrackNumber
        {
            get { return _trackNumber; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Expecting a track number greater than zero.");
                _trackNumber = value;
            }
        }
        /// <summary>
        /// Get/Set the tracks length in milliseconds.
        /// </summary>
        public int Length
        {
            get { return _length; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Expecting a length greater than zero.");
                _length = value;
            }
        }
        /// <summary>
        /// Get/Set if this track is explicit.
        /// </summary>
        public bool IsExplicit { get { return _explicit; } set { _explicit = value; } }
        #endregion
        #region Factory
        /// <summary>
        /// Creates an empty <see cref="Track"/> object.
        /// </summary>
        public Track()
        {
            this._title = null;
            this._artist = null;
            this._album = null;
            this._year = -1;
            this._trackNumber = -1;
            this._length = -1;
            this._files = new List<File>();
            this._cover = null;
            this._similarTracks = new List<Track>();
        }
        /// <summary>
        /// Creates a <see cref="Track"/> object with the specified <code>id</code>.
        /// </summary>
        /// <param name="id">A 32-character hex string or a Spotify URI.</param>
        public Track(string id) : this(id, null, null, null)
        {
        }
        /// <summary>
        /// Creates a <see cref="Track"/> object with the specified <code>id</code>, <code>title</code>, <code>artist</code> and <code>album</code>.
        /// </summary>
        /// <param name="id">A 32-character hex string or a Spotify URI.</param>
        /// <param name="title">Title of the track.</param>
        /// <param name="artist">Artist of the track.</param>
        /// <param name="album">Album of the track.</param>
        public Track(string id, string title, Artist artist, Album album) : base(id)
        {
            /* Set object properties. */
            this._title = title;
            this._artist = artist;
            this._album = album;
            this._year = -1;
            this._trackNumber = -1;
            this._length = -1;
            this._files = new List<File>();
            this._cover = null;
            this._similarTracks = new List<Track>();
        }
        #endregion
        #region Methods
        /// <summary>
        /// Add a <see cref="File"/> to the list of files.
        /// </summary>
        /// <param name="file"></param>
        public void AddFile(File file)
        {
            this._files.Add(file);
        }

        public override bool Equals(Object obj)
        {
            if (obj is Track)
            {
                Track t = obj as Track;
                if (this.Id == t.Id)
                    return true;
                foreach (string rId in this.Redirects)
                {
                    if (rId == t.Id)
                        return true;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (this._id != null) ? this._id.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return string.Format("[Track: {0}, {1}, {2}]", this.Artist, this.Album, this.Title);
        }
        #endregion
    }
}
