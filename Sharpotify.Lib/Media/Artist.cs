using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharpotify.Util;

namespace Sharpotify.Media
{
    public class Artist : Media
    {
        #region Fields
        /// <sumary>
        /// Name of this artist.
        /// </sumary>
        private string _name;
        /// <sumary>
        /// The identifier for this artists portrait image (40-character string).
        /// </sumary>
        private string _portrait;
        /// <sumary>
        /// A List of genres.
        /// </sumary>
        private List<string> _genres;
        /// <sumary>
        /// A List of years active.
        /// </sumary>
        private List<string> _yearsActive;
        /// <sumary>
        /// A List of Biographies.
        /// </sumary>
        private List<Biography> _bios;
        /// <sumary>
        /// A List of Albums.
        /// </sumary>
        private List<Album> _albums;
        /// <sumary>
        /// A List of similar Artists.
        /// </sumary>
        private List<Artist> _similarArtists;
        #endregion
        #region Properties
        /// <summary>
        /// Create a link from this artist.
        /// </summary>
        public Link Link { get { return Sharpotify.Media.Link.Create(this); } }
        /// <summary>
        /// Get/Set the artists name.
        /// </summary>
        public string Name { get { return this._name; } set { this._name = value; } }
        /// <summary>
        /// Get/Set the artists portrait image identifier.
        /// </summary>
        public string Portrait { get { return this._portrait; } set { this._portrait = value; } }
        /// <summary>
        /// Get/Set genres for this artist.
        /// </summary>
        public List<string> Genres { get { return this._genres; } set { this._genres = value; } }
        /// <summary>
        /// Get/Set active years for this artist.
        /// </summary>
        public List<string> YearsActive { get { return this._yearsActive; } set { this._yearsActive = value; } }
        /// <summary>
        /// Get/Set biographies for this artist.
        /// </summary>
        public List<Biography> Bios { get { return this._bios; } set { this._bios = value; } }
        /// <summary>
        /// Get/Set albums for this artist.
        /// </summary>
        public List<Album> Albums { get { return this._albums; } set { this._albums = value; } }
        /// <summary>
        /// Get/et similar artists for this artist.
        /// </summary>
        public List<Artist> SimilarArtists { get { return this._similarArtists; } set { this._similarArtists = value; } }
        #endregion
        #region Factory
        /// <summary>
        /// Creates an empty <see cref="Artist"/> object.
        /// </summary>
        public Artist()
        {
            this._name = null;
            this._portrait = null;
            this._genres = new List<string>();
            this._yearsActive = new List<string>();
            this._bios = new List<Biography>();
            this._albums = new List<Album>();
            this._similarArtists = new List<Artist>();
        }
        /// <summary>
        /// Creates an <see cref="Artist"/> object with the specified <code>id</code>.
        /// </summary>
        /// <param name="id">A 32-character hex string or a Spotify URI.</param>
        public Artist(string id) : this(id, null)
        {
        }
        /// <summary>
        /// Creates an <see cref="Artist"/> object with the specified <code>id</code> and <code>name</code>.
        /// </summary>
        /// <param name="id">A 32-character hex string or a Spotify URI.</param>
        /// <param name="name">Name of the artist or <code>null</code>.</param>
        public Artist(string id, string name) : base(id)
        {
            this._name = name;
            this._portrait = null;
            this._genres = new List<string>();
            this._yearsActive = new List<string>();
            this._bios = new List<Biography>();
            this._albums = new List<Album>();
            this._similarArtists = new List<Artist>();
        }
        #endregion
        #region Methods
        /// <summary>
        /// Determines if an object is equal to this <see cref="Artist"/> object.
        /// If both objects are <see cref="Artist"/> objects, it will compare their identifiers.
        /// </summary>
        /// <param name="obj">Another object to compare.</param>
        /// <returns>true of the objects are equal, false otherwise.</returns>
        public override bool Equals(Object obj)
        {
            if (obj is Artist)
            {
                Artist a = obj as Artist;
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
        /// Return the hash code of this <see cref="Artist"/> object. This will give the value returned
        /// by the hashCode method of the identifier string.
        /// </summary>
        /// <returns>The <see cref="Artist"/> objects hash code.</returns>
        public override int GetHashCode()
        {
            return (this.Id != null) ? this.Id.GetHashCode() : 0;
        }
        public override string ToString()
        {
            return string.Format("[Artist: {0}]", this.Name);
        }
        #endregion
    }
}
