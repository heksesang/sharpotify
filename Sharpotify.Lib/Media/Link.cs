using System;
using System.Text.RegularExpressions;

using Sharpotify.Util;

namespace Sharpotify.Media
{
    public class Link
    {
        #region Utils
        /// <summary>
        /// Different possible link types.
        /// </summary>
        public enum LinkType
        { 
            ARTIST, ALBUM, TRACK, PLAYLIST, SEARCH
        }
        /// <summary>
        /// An exception that is thrown if parsing a Spotify URI fails. 
        /// </summary>
	    public class InvalidSpotifyURIException : Exception 
        {
        }
        #endregion

        #region Fields
        /// <summary>
        /// A regular expression to match artist, album and track URIs:
        /// <pre>spotify:(artist|album|track):([0-9A-Za-z]{22})</pre>
        /// </summary>
        private static Regex mediaPattern = new Regex("spotify:(artist|album|track):([0-9A-Za-z]{22})");
        /// <summary>
        /// A regular expression to match playlist URIs:
        /// <pre>spotify:user:([^:]+):playlist:([0-9A-Za-z]{22})</pre>
        /// </summary>
        private static Regex playlistPattern = new Regex("spotify:user:([^:]+):playlist:([0-9A-Za-z]{22})");
        /// <summary>
        /// A regular expression to match search URIs:
        /// <pre>spotify:search:([^\\s]+)</pre>
        /// </summary>
        private static Regex searchPattern = new Regex("spotify:search:([^\\s]+)");

        /// <summary>
        /// The <see cref="Link.Type"/> of this link.
        /// </summary>
        private LinkType _type;
        /// <summary>
        /// The id of this link, if it's an artist, album or track link.
        /// </summary>
        private string _id;
        /// <summary>
        /// The user of a playlist link.
        /// </summary>
        private string _user;
        /// <summary>
        /// The search query of a search link.
        /// </summary>
        private string _query;
        #endregion
        #region Properties
        /// <summary>
        /// Get the <see cref="Link.Type"/> of this link.
        /// </summary>
        public LinkType Type
        {
            get { return this._type; }
        }
        /// <summary>
        /// Check if this link is an artist link.
        /// </summary>
        public bool IsArtistLink
        {
            get { return (this._type == LinkType.ARTIST); }
        }
        /// <summary>
        /// Check if this link is an album link.
        /// </summary>
        public bool IsAlbumLink
        {
            get { return (this._type == LinkType.ALBUM); }
        }
        /// <summary>
        /// Check if this link is a track link.
        /// </summary>
        public bool IsTrackLink
        {
            get { return (this._type == LinkType.TRACK); }
        }
        /// <summary>
        /// Check if this link is a playlist link.
        /// </summary>
        public bool IsPlaylistLink
        {
            get { return (this._type == LinkType.PLAYLIST); }
        }
        /// <summary>
        /// Check if this link is a search link.
        /// </summary>
        public bool IsSearchLink
        {
            get { return (this._type == LinkType.SEARCH); }
        }

        /// <summary>
        /// Get the id of this link.
        /// </summary>
        public string Id
        {
            get {
                if (this._id == null)
                    throw new ArgumentException("Link doesn't have an id!");
                return this._id; 
            }
        }
        /// <summary>
        /// Get the user of this playlist link.
        /// </summary>
        public string User
        {
            get
            {
                if (!this.IsPlaylistLink)
                    throw new ArgumentException("Link is not a playlist link!");
                return this._user;
            }
        }
        /// <summary>
        /// Get the query of this search link.
        /// </summary>
        public string Query
        {
            get
            {
                if (!this.IsSearchLink)
                    throw new ArgumentException("Link is not a search link!");
                return this._query;
            }
        }
        /// <summary>
        /// Gets the Spotify URI representation of this link.
        /// </summary>
        public string AsString
        {
            get 
            {
                if (this.IsPlaylistLink)
                {
                    return string.Format(
                        "spotify:user:%s:playlist:%s",
                        this._user, Link.ToBase62(this._id)
                    );
                }
                else if (this.IsSearchLink)
                {
                    try
                    {
                        return string.Format(
                            "spotify:search:%s",
                            System.Web.HttpUtility.UrlDecode(this._query, System.Text.Encoding.UTF8)
                        );
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                else
                {
                    return string.Format(
                        "spotify:%s:%s", this._type.ToString().ToLower().Replace("linktype.", ""),
                        Link.ToBase62(this._id)
                    );
                }
            }
        }
        /// <summary>
        /// Gets the HTTP Spotify URI representation of this link.
        /// </summary>
        public string AsHttpLink
        {
            get 
            {
                if (this.IsPlaylistLink)
                {
                    return string.Format(
                        "http://open.spotify.com/user/%s/playlist/%s",
                        this._user, Link.ToBase62(this._id)
                    );
                }
                else if (this.IsSearchLink)
                {
                    try
                    {
                        return string.Format(
                            "http://open.spotify.com/search/%s",
                            System.Web.HttpUtility.UrlDecode(this._query, System.Text.Encoding.UTF8)
                        );
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                else
                {
                    return string.Format(
                        "http://open.spotify.com/%s/%s",
                        this._type.ToString().ToLower().Replace("linktype.", ""), 
                        Link.ToBase62(this._id)
                    );
                }
            }
        }
        #endregion
        #region Factory
        /// <summary>
        /// Create a <see cref="Link"/> using the given parameters.
        /// </summary>
        /// <param name="type">The <see cref="Link.Type"/> to use.</param>
        /// <param name="id">The id to set (for artists, albums and tracks) or null.</param>
        /// <param name="user">The user to set (for playlists) or null.</param>
        /// <param name="query">The search query to set (for search) or null.</param>
        private Link(LinkType type, string id, string user, string query)
        {
            this._type = type;
            this._id = id;
            this._user = user;
            this._query = query;
        }

        private Link(string uri)
        {
            /* Regex for matching Spotify URIs. */
            Match mediaMatcher = mediaPattern.Match(uri);
            Match playlistMatcher = playlistPattern.Match(uri);
            Match searchMatcher = searchPattern.Match(uri);

            /* Check if URI matches artist/album/track pattern. */
            if (mediaMatcher.Success)
            {
                string type = mediaMatcher.Groups[1].Value;

                if (type.CompareTo("artist") == 0)
                    this._type = LinkType.ARTIST;
                else if (type.CompareTo("album") == 0)
                    this._type = LinkType.ALBUM;
                else if (type.CompareTo("track") == 0)
                    this._type = LinkType.TRACK;
                else
                    throw new InvalidSpotifyURIException();

                this._id = Link.ToHex(mediaMatcher.Groups[2].Value);
                this._user = null;
                this._query = null;
            }
            /* Check if URI matches playlist pattern. */
            else if (playlistMatcher.Success)
            {
                this._type = LinkType.PLAYLIST;
                this._user = playlistMatcher.Groups[1].Value;
                this._id = Link.ToHex(playlistMatcher.Groups[2].Value);
                this._query = null;
            }
            /* Check if URI matches search pattern. */
            else if (searchMatcher.Success)
            {
                this._type = LinkType.SEARCH;
                this._id = null;
                this._user = null;

                try
                {
                    this._query = System.Web.HttpUtility.UrlDecode(searchMatcher.Groups[1].Value, System.Text.Encoding.UTF8);
                }
                catch (Exception)
                {
                    throw new InvalidSpotifyURIException();
                }
            }
        }
        /// <summary>
        /// Create a <see cref="Link"/> from a Spotify URI.
        /// </summary>
        /// <param name="uri">A Spotify URI to parse.</param>
        /// <returns>A <see cref="Link"/> object</returns>
        public static Link Create(string uri) 
        {
		    return new Link(uri);
	    }
        /// <summary>
        /// Create a <see cref="Link"/> from an <see cref="Artist"/>.
        /// </summary>
        /// <param name="artist">An <see cref="Artist"/> object.</param>
        /// <returns>A <see cref="Link"/> object</returns>
        public static Link Create(Artist artist)
        {
            return new Link(LinkType.ARTIST, artist.Id, null, null);
        }
        /// <summary>
        /// Create a <see cref="Link"/> from an <see cref="Album"/>.
        /// </summary>
        /// <param name="album">An <see cref="Album"/> object.</param>
        /// <returns>A <see cref="Link"/> object</returns>
        public static Link Create(Album album)
        {
            return new Link(LinkType.ALBUM, album.Id, null, null);
        }
        /// <summary>
        /// Create a <see cref="Link"/> from a <see cref="Track"/>.
        /// </summary>
        /// <param name="track">An <see cref="Track"/> object.</param>
        /// <returns>A <see cref="Link"/> object</returns>
        public static Link Create(Track track)
        {
            return new Link(LinkType.TRACK, track.Id, null, null);
        }
        /// <summary>
        /// Create a <see cref="Link"/> from a <see cref="Playlist"/>.
        /// </summary>
        /// <param name="playlist">An <see cref="Playlist"/> object.</param>
        /// <returns>A <see cref="Link"/> object</returns>
        public static Link Create(Playlist playlist)
        {
            return new Link(LinkType.PLAYLIST, playlist.Id, playlist.Author, null);
        }
        /// <summary>
        /// Create a <see cref="Link"/> from a <see cref="Result"/>.
        /// </summary>
        /// <param name="result">An <see cref="Result"/> object.</param>
        /// <returns>A <see cref="Link"/> object</returns>
        public static Link Create(Result result)
        {
            return new Link(LinkType.SEARCH, null, null, result.Query);
        }
        #endregion
        #region Methods
        /// <summary>
        /// Convert a base-62 encoded id into a hexadecimal id.
        /// </summary>
        /// <param name="base62">A base-62 encoded id.</param>
        /// <returns>A hexadecimal id.</returns>
        private static string ToHex(string base62)
        {
            string hex = BaseConvert.Convert(base62, 62, 16);

            /* Prepend zeroes until hexadecimal string length is 32. */
            while (hex.Length < 32)
            {
                hex.Insert(0, "0");
            }

            return hex;
        }
        /// <summary>
        /// Convert a hexadecimal id into a base-62 encoded id.
        /// </summary>
        /// <param name="hex">A hexadecimal id.</param>
        /// <returns>A base-62 encoded id.</returns>
        private static string ToBase62(string hex)
        {
            string uri = BaseConvert.Convert(hex, 16, 62);

            /* Prepend zeroes until base-62 string length is 22. */
            while (uri.Length < 22)
            {
                uri.Insert(0, "0");
            }

            return uri;
        }
        /// <summary>
        /// Returns the string representation of this link.
        /// </summary>
        /// <returns>A Spotify URI string</returns>
        public override string ToString()
        {
            return this.AsString;
        }
        #endregion
    }
}
