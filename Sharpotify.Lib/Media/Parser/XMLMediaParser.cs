using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using Sharpotify.Exceptions;

namespace Sharpotify.Media.Parser
{
    public class XMLMediaParser : XMLParser
    {
        #region Constants
        private const int SUPPORTED_ALBUM_VERSION = 1;
        private const int SUPPORTED_ARTIST_VERSION = 1;
        private const int SUPPORTED_RESULT_VERSION = 1;
        #endregion
        #region Factory
        private XMLMediaParser(Stream stream) : base(stream)
        {
        }
        #endregion
        #region Private Methods
        private object Parse()
        {
            string name;
            if (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

                /* Check current element name and start parsing it. */
                if (name.Equals("result"))
                {
                    return this.ParseResult();
                }
                else if (name.Equals("toplist"))
                {
                    return this.ParseResult();
                }
                else if (name.Equals("artist"))
                {
                    return this.ParseArtist();
                }
                else if (name.Equals("album"))
                {
                    return this.ParseAlbum();
                }
                else if (name.Equals("track"))
                {
                    return this.ParseTrack();
                }
                else
                {
                    throw new XMLParserException("Unexpected element '<" + name + ">'");
                }
            }
            throw new XMLParserException("Reader is not on a start element!");
        }
        /// <summary>
        /// Parse the input stream as a <see cref="Result"/>.
        /// </summary>
        /// <returns></returns>
        private Result ParseResult() 
        {
            Result result = new Result();
            string name;

            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;
                /* Process depending on element name. */
                if (name.Equals("version"))
                {
                    int version = this.GetElementInteger();

                    /* Check version. */
                    if (version > SUPPORTED_RESULT_VERSION)
                    {
                        throw new XMLParserException("Unsupported <result> version " + version);
                    }
                }
                else if (name.Equals("did-you-mean"))
                {
                    result.Suggestion = this.GetElementString();
                }
                else if (name.Equals("total-artists"))
                {
                    result.TotalArtists = this.GetElementInteger();
                }
                else if (name.Equals("total-albums"))
                {
                    result.TotalAlbums = this.GetElementInteger();
                }
                else if (name.Equals("total-tracks"))
                {
                    result.TotalTracks = this.GetElementInteger();
                }
                else if (name.Equals("artists"))
                {
                    result.Artists = ParseArtists();
                }
                else if (name.Equals("albums"))
                {
                    result.Albums = ParseAlbums();
                }
                else if (name.Equals("tracks"))
                {
                    result.Tracks = ParseTracks();
                }
                else
                {
                    throw new XMLParserException("Unexpected element '<" + name + ">'");
                }

                this.Next();
            }
            return result;
        }
        /// <summary>
        /// Parse the input stream as a list of artists.
        /// </summary>
        /// <returns>A List of <see cref="Artist"/> objects.</returns>
        private List<Artist> ParseArtists() 
        {
            List<Artist> artists = new List<Artist>();
            string name;

            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

                /* Process depending on element name. */
                if (name.Equals("artist"))
                {
                    artists.Add(ParseArtist());
                }
                else
                {
                    throw new XMLParserException("Unexpected element '<" + name + ">'");
                }

                this.Next();
            }

            return artists;
        }
        /// <summary>
        /// Parse the input stream as a list of albums.
        /// </summary>
        /// <returns>A List of <see cref="Album"/> objects.</returns>
        private List<Album> ParseAlbums() 
        {
            List<Album> albums = new List<Album>();
            string name;

            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

                /* Process depending on element name. */
                if (name.Equals("album"))
                {
                    albums.Add(ParseAlbum());
                }
                else
                {
                    throw new XMLParserException("Unexpected element '<" + name + ">'");
                }

                this.Next();
            }

            return albums;
        }
        /// <summary>
        /// Parse the input stream as a list of tracks.
        /// </summary>
        /// <returns>A List of <see cref="Track"/> objects.</returns>
        private List<Track> ParseTracks()
        {
            List<Track> tracks = new List<Track>();
            string name;

            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

                /* Process depending on element name. */
                if (name.Equals("track"))
                {
                    tracks.Add(ParseTrack());
                }
                else
                {
                    throw new XMLParserException("Unexpected element '<" + name + ">'");
                }

                this.Next();
            }

            return tracks;
        }
        /// <summary>
        /// Parse the input stream as an <see cref="Artist"/>.
        /// </summary>
        /// <returns>An <see cref="Artist"/> object.</returns>
        private Artist ParseArtist()
        {
            Artist artist = new Artist();
            string name;

            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

                /* Process depending on element name. */
                if (name.Equals("version"))
                {
                    int version = this.GetElementInteger();

                    /* Check version. */
                    if (version > SUPPORTED_ARTIST_VERSION)
                    {
                        throw new XMLParserException("Unsupported <album> version " + version);
                    }
                }
                else if(name.Equals("id")){
                    /* TODO: handle different ID types. */
                    if (this.GetAttributeString("type") == null)
                    {
                        artist.Id = this.GetElementString();
                    }
                }
                else if (name.Equals("redirect"))
                {
                    artist.AddRedirect(this.GetElementString());
                }
                else if (name.Equals("name"))
                {
                    artist.Name = this.GetElementString();
                }
                else if (name.Equals("portrait"))
                {
                    artist.Portrait = ParseImage();
                }
                else if (name.Equals("genres"))
                {
                    string[] genres = this.GetElementString().Split(',');
                    artist.Genres = new List<string>(genres);
                }
                else if (name.Equals("years-active"))
                {
                    string[] years = this.GetElementString().Split(',');
                    artist.YearsActive = new List<string>(years);
                }
                else if (name.Equals("popularity"))
                {
                    artist.Popularity = this.GetElementFloat();
                }
                else if (name.Equals("bios"))
                {
                    artist.Bios = ParseBios();
                }
                else if (name.Equals("similar-artists"))
                {
                    artist.SimilarArtists = ParseArtists();
                }
                else if (name.Equals("albums"))
                {
                    artist.Albums = ParseAlbums();
                }
                else if (name.Equals("restrictions"))
                {
                    artist.Restrictions = ParseRestrictions();
                }
                else if (name.Equals("external-ids"))
                {
                    artist.ExternalIds = ParseExternalIds();
                }
                else
                {
                    throw new XMLParserException("Unexpected element '<" + name + ">'");
                }
                
                this.Next();
            }

            return artist;
        }
        private Album ParseAlbum() 
        {
            Album  album = new Album();
            string name;

            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

                /* Process depending on element name. */
                if (name.Equals("version"))
                {
                    int version = this.GetElementInteger();

                    /* Check version. */
                    if (version > SUPPORTED_ALBUM_VERSION)
                    {
                        throw new XMLParserException("Unsupported <album> version " + version);
                    }
                }
                else if(name.Equals("id"))
                {
                    /* TODO: handle different ID types. */
                    if (this.GetAttributeString("type") == null)
                    {
                        album.Id = this.GetElementString();
                    }
                    
                }
                else if (name.Equals("redirect"))
                {
                    album.AddRedirect(this.GetElementString());
                }
                else if (name.Equals("name"))
                {
                    album.Name = this.GetElementString();
                }
                else if (name.Equals("artist") || name.Equals("artist-name"))
                {
                    Artist artist = (album.Artist != null) ? album.Artist : new Artist();
                    /* Get artist name. */
                    artist.Name = this.GetElementString();
                    album.Artist = artist;
                }
                else if (name.Equals("artist-id"))
                {
                    Artist artist = (album.Artist != null) ? album.Artist : new Artist();
                    artist.Id = this.GetElementString();
                    album.Artist = artist;
                }
                else if (name.Equals("album-type"))
                {
                    album.Type = this.GetElementString();
                }
                else if (name.Equals("cover"))
                {
                    album.Cover = this.GetElementString();
                }
                else if (name.Equals("cover-small"))
                {
                    album.CoverSmall = this.GetElementString();
                }
                else if (name.Equals("cover-large"))
                {
                    album.CoverLarge = this.GetElementString();
                }
                else if (name.Equals("popularity"))
                {
                    album.Popularity = this.GetElementFloat();
                }
                else if (name.Equals("review"))
                {
                    album.Review = this.GetElementString();
                }
                else if (name.Equals("year") || name.Equals("released"))
                {
                    album.Year = this.GetElementInteger();
                }
                /* TODO: currently skipped. */
                else if (name.Equals("copyright"))
                {
                    /* Go to next element and check if it is a start element. */
                    this.Next();
                    while (this.reader.IsStartElement())
                    {
                        name = this.reader.LocalName;

                        /* Process depending on element name. */
                        if (name.Equals("c"))
                        {
                            /* Skip text. */
                            this.GetElementString();
                        }
                        else if (name.Equals("p"))
                        {
                            /* Skip text. */
                            this.GetElementString();
                        }
                        else
                        {
                            throw new XMLParserException("Unexpected element '<" + name + ">'");
                        }
                        this.Next();
                    }
                }
                /* TODO: currently skipped. */
                else if (name.Equals("links"))
                {
                    SkipLinks();
                }
                else if (name.Equals("restrictions"))
                {
                    album.Restrictions = ParseRestrictions();
                }
                /* TODO: currently skipped. */
                else if (name.Equals("availability"))
                {
                    SkipAvailability();
                }
                /* Seems to be deprecated. */
                else if (name.Equals("allowed"))
                {
                    /* Skip text. */
                    this.GetElementString();
                }
                /* Seems to be deprecated. */
                else if (name.Equals("forbidden"))
                {
                    /* Skip text. */
                    this.GetElementString();
                }
                else if (name.Equals("genres"))
                {
                    /* Skip text. */
                    this.GetElementString();
                }
                else if (name.Equals("discs"))
                {
                    List<Disc> discs = new List<Disc>();

                    /* Go to next element and check if it is a start element. */
                    this.Next();
                    while (this.reader.IsStartElement())
                    {
                        name = this.reader.LocalName;

                        /* Process depending on element name. */
                        if (name.Equals("disc"))
                        {
                            List<Track> tracks = new List<Track>();
                            Disc disc = new Disc();

                            /* Go to next element and check if it is a start element. */
                            this.Next();
                            while (this.reader.IsStartElement())
                            {
                                name = this.reader.LocalName;

                                if (name.Equals("disc-number"))
                                {
                                    disc.Number = this.GetElementInteger();
                                }
                                else if (name.Equals("name"))
                                {
                                    disc.Name = this.GetElementString();
                                }
                                else if (name.Equals("track"))
                                {
                                    Track track = ParseTrack();

                                    track.Album = album;
                                    track.Cover = album.Cover;

                                    tracks.Add(track);
                                }
                                else
                                {
                                    throw new XMLParserException("Unexpected element '<" + name + ">'");
                                }

                                this.Next();
                            }

                            /* Set disc tracks. */
                            disc.Tracks = tracks;
                            discs.Add(disc);
                        }
                        else
                        {
                            throw new XMLParserException("Unexpected element '<" + name + ">'");
                        }

                        this.Next();
                    }

                    album.Discs = discs;
                }
                else if (name.Equals("similar-albums"))
                {
                    List<Album> similarAlbums = new List<Album>();

                    this.Next();
                    while (this.reader.IsStartElement())
                    {
                        name = this.reader.LocalName;

                        /* Process depending on element name. */
                        if (name.Equals("id"))
                        {
                            similarAlbums.Add(new Album(this.GetElementString()));
                        }
                        else
                        {
                            throw new XMLParserException("Unexpected element '<" + name + ">'");
                        }

                        this.Next();
                    }

                    /* Set similar albums. */
                    album.SimilarAlbums = similarAlbums;
                }
                else if (name.Equals("external-ids"))
                {
                    album.ExternalIds = ParseExternalIds();
                }
                else
                {
                    throw new XMLParserException("Unexpected element '<" + name + ">'");
                }
                
                this.Next();
            }

            return album;
        }
        private Track ParseTrack()
        {
            Track  track = new Track();
            string name;

            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

                /* Process depending on element name. */
                if(name.Equals("id"))
                {
                    /* TODO: handle different ID types. */
                    if(this.GetAttributeString("type") == null)
                    {
                        track.Id = this.GetElementString();
                    }
                }
                else if (name.Equals("redirect"))
                {
                    track.AddRedirect(this.GetElementString());
                }
                /* TODO: currently skipped. */
                else if (name.Equals("redirect"))
                {
                    /* Skip text. */
                    this.GetElementString();
                }
                else if (name.Equals("title") || name.Equals("name"))
                {
                    track.Title = this.GetElementString();
                }
                else if (name.Equals("artist"))
                {
                    Artist artist = (track.Artist != null) ? track.Artist : new Artist();
                    /* Get artist name. */
                    artist.Name = this.GetElementString();
                    track.Artist = artist;
                }
                else if (name.Equals("artist-id"))
                {
                    Artist artist = (track.Artist != null) ? track.Artist : new Artist();
                    artist.Id = this.GetElementString();
                    track.Artist = artist;
                }
                else if (name.Equals("album"))
                {
                    Album album = (track.Album != null) ? track.Album : new Album();
                    /* Get album name. */
                    album.Name = this.GetElementString();
                    track.Album = album;
                }
                else if (name.Equals("album-id"))
                {
                    Album album = (track.Album != null) ? track.Album : new Album();
                    album.Id = this.GetElementString();
                    track.Album = album;
                }
                else if (name.Equals("album-artist"))
                {
                    Album album = (track.Album != null) ? track.Album : new Album();
                    Artist artist = (track.Artist != null) ? track.Artist : new Artist();
                    artist.Name = this.GetElementString();
                    album.Artist = artist;
                    track.Album = album;
                }
                else if (name.Equals("album-artist-id"))
                {
                    Album album = (track.Album != null) ? track.Album : new Album();
                    Artist artist = (track.Artist != null) ? track.Artist : new Artist();
                    artist.Id = this.GetElementString();
                    album.Artist = artist;
                    track.Album = album;
                }
                else if (name.Equals("year"))
                {
                    track.Year = this.GetElementInteger();
                }
                else if (name.Equals("track-number"))
                {
                    track.TrackNumber = this.GetElementInteger();
                }
                else if (name.Equals("length"))
                {
                    int length = this.GetElementInteger();

                    if (length > 0)
                    {
                        track.Length = length;
                    }
                }
                else if (name.Equals("files"))
                {
                    track.Files = ParseFiles();
                }
                /* TODO: currently skipped. */
                else if (name.Equals("links"))
                {
                    SkipLinks();
                }
                /* TODO: currently skipped. */
                else if (name.Equals("album-links"))
                {
                    SkipLinks();
                }
                else if (name.Equals("cover"))
                {
                    track.Cover = this.GetElementString();
                }
                else if (name.Equals("cover-small"))
                {
                    track.CoverSmall = this.GetElementString();
                }
                else if (name.Equals("cover-large"))
                {
                    track.CoverLarge = this.GetElementString();
                }
                else if (name.Equals("popularity"))
                {
                    track.Popularity = this.GetElementFloat();
                }
                else if (name.Equals("restrictions"))
                {
                    track.Restrictions = this.ParseRestrictions();
                }
                else if (name.Equals("explicit"))
                {
                    track.IsExplicit = this.GetElementBoolean();
                }
                /* Seems to be deprecated. */
                else if (name.Equals("allowed"))
                {
                    /* Skip text. */
                    this.GetElementString();
                }
                /* Seems to be deprecated. */
                else if (name.Equals("forbidden"))
                {
                    /* Skip text. */
                    this.GetElementString();
                }
                else if (name.Equals("similar-tracks"))
                {
                    List<Track> similarTracks = new List<Track>();

                    /* Go to next element and check if it is a start element. */
                    this.Next();
                    while (this.reader.IsStartElement())
                    {
                        name = this.reader.LocalName;

                        /* Process depending on element name. */
                        if (name.Equals("id"))
                        {
                            similarTracks.Add(new Track(this.GetElementString()));
                        }
                        else
                        {
                            throw new XMLParserException("Unexpected element '<" + name + ">'");
                        }

                        this.Next();
                    }

                    /* Set similar tracks. */
                    track.SimilarTracks = similarTracks;
                }
                else if (name.Equals("alternatives"))
                {
                    var tracks = ParseTracks();
                }
                else if (name.Equals("external-ids"))
                {
                    track.ExternalIds = ParseExternalIds();
                }
                else
                {
                    throw new XMLParserException("Unexpected element '<" + name + ">'");
                }
                
                this.Next();
            }

            /* If album artist of this track is not yet set, then set it. */
            if(track.Album != null && track.Album.Artist == null)
            {
                track.Album.Artist = track.Artist;
            }

            return track;
        }
        private string ParseImage() 
        {
            string id = null;
            string name;

            if (reader.IsEmptyElement)
                return null;

            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

                /* Process depending on element name. */
                if(name.Equals("id")){
                    id = this.GetElementString();
                }
                else if(name.Equals("width")){
                    this.GetElementString(); /* Skip. */
                }
                else if(name.Equals("height")){
                    this.GetElementString(); /* Skip. */
                }
                else if (name.Equals("small")){
                    this.GetElementString(); /* Skip. */
                }
                else if (name.Equals("large"))
                {
                    this.GetElementString(); /* Skip. */
                }
                else
                    throw new XMLParserException("Unexpected element '<" + name + ">'");

                this.Next();
            }

            /* If the reader is not at an end element, it is at some character event. */
            if(this.reader.NodeType != XmlNodeType.EndElement )
            {
                /* Read image id from element text (special case). */
                id = this.GetElementString().Trim();

                /* Skip to end element. */
                //this.Next();
            }

            return id;
        }
        private List<Biography> ParseBios()
        {
            List<Biography> bios = new List<Biography>();
            string name;

            if (reader.IsEmptyElement)
                return bios;
            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

                /* Process depending on element name. */
                if(name.Equals("bio"))
                {
                    Biography bio = new Biography();

                    /* Go to next element and check if it is a start element. */
                    this.Next();
                    while (this.reader.IsStartElement())
                    {
                        name = this.reader.LocalName;

                        /* Process depending on element name. */
                        if(name.Equals("text"))
                            bio.Text = this.GetElementString();
                        else if(name.Equals("portraits"))
                        {
                            List<string> portraits = new List<string>();

                            if (!this.reader.IsEmptyElement)
                            {
                                /* Go to next element and check if it is a start element. */
                                this.Next();
                                while (this.reader.IsStartElement())
                                {
                                    name = this.reader.LocalName;

                                    /* Process depending on element name. */
                                    if (name.Equals("portrait"))
                                        portraits.Add(ParseImage());
                                    else
                                        throw new XMLParserException("Unexpected element '<" + name + ">'");

                                    this.Next();
                                }

                                /* Add portraits to biography. */
                                bio.Portraits = portraits;
                            }
                        }
                        else
                            throw new XMLParserException("Unexpected element '<" + name + ">'");

                        this.Next();
                    }

                    /* Add biograhpy to list. */
                    bios.Add(bio);
                }
                else
                    throw new XMLParserException("Unexpected element '<" + name + ">'");

                this.Next();
            }

            return bios;
        }
        private List<File> ParseFiles() 
        {
            List<File> files = new List<File>();
            string name;

            if (reader.IsEmptyElement)
                return files;
            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;
                if (!this.reader.HasAttributes)
                    break;
                /* Process depending on element name. */
                if(name.Equals("file")){
                    files.Add(new File(
                        GetAttributeString("id"), GetAttributeString("format")
                    ));

                    /* Skip to end element, since we only read the attributes. */
                    //this.Next();
                }
                else
                    throw new XMLParserException("Unexpected element '<" + name + ">'");
        
                this.Next();
            }

            return files;
        }
        private List<Restriction> ParseRestrictions()
        {
            List<Restriction> restrictions = new List<Restriction>();
            string name;

            if (reader.IsEmptyElement)
                return restrictions;

            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

                /* Process depending on element name. */
                if(name.Equals("restriction")){
                    restrictions.Add(new Restriction(
                        GetAttributeString("allowed"),
                        GetAttributeString("forbidden"),
                        GetAttributeString("catalogues")
                    ));

                    /* Skip to end element since we only read the attributes. */
                    //this.Next();
                }
                else
                    throw new XMLParserException("Unexpected element '<" + name + ">'");

                this.Next();
            }

            return restrictions;
        }
        private Dictionary<string, string> ParseExternalIds() 
        {
            Dictionary<string, string> externalIds = new Dictionary<string, string>();
            string name;

            if (reader.IsEmptyElement)
                return externalIds;

            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

                /* Process depending on element name. */
                if(name.Equals("external-id"))
                {
                    string key = GetAttributeString("type");
                    string value = GetAttributeString("id");
                    if (!string.IsNullOrEmpty(key))
                    {
                        if (externalIds.ContainsKey(key))
                            externalIds[key] = value;
                        else
                            externalIds.Add(key, value);
                    }

                    /* Skip to end element since we only read the attributes. */
                    //this.Next();
                }
                else
                    throw new XMLParserException("Unexpected element '<" + name + ">'");

                this.Next();
            }

            return externalIds;
        }
        private void SkipLinks()
        {
            string name;

            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

                /* Process depending on element name. */
                if(name.Equals("link"))
                {
                    /* Skip text. */
                    this.GetElementString();
                }
                else
                    throw new XMLParserException("Unexpected element '<" + name + ">'");

                this.Next();
            }
        }
        private void SkipAvailability() 
        {
            string name;

            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

                /* Process depending on element name. */
                if(name.Equals("territories"))
                {
                    /* Skip text. */
                    this.GetElementString();
                }
                else
                    throw new XMLParserException("Unexpected element '<" + name + ">'");

                this.Next();
            }
        }
        #endregion
        #region Public Methods
        /// <summary>
        /// Parse <code>xml</code> into an generic object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        protected static T Parse<T>(byte[] xml) where T : class
        {
            object result = Parse(xml);
            if (result is T)
                return (T)result;

            return null;
        }
        /// <summary>
        /// Parse <code>xml</code> into an object.
        /// </summary>
        /// <param name="xml">The xml as bytes.</param>
        /// <returns>Returns the object.</returns>
        public static object Parse(byte[] xml)
        {
            XMLMediaParser parser = new XMLMediaParser(new MemoryStream(xml, 0, xml.Length - 1));
            return parser.Parse();
        }
        /// <summary>
        /// Parse <code>xml</code> into a <see cref="Result"/> object.
        /// </summary>
        /// <param name="xml">The xml as bytes.</param>
        /// <returns>A <see cref="Result"/> object if successful, null if not.</returns>
        public static Result ParseResult(byte[] xml)
        {
            return Parse<Result>(xml);
        }
        /// <summary>
        /// Parse <code>xml</code> into a <see cref="Artist"/> object.
        /// </summary>
        /// <param name="xml">The xml as bytes.</param>
        /// <returns>A <see cref="Artist"/> object if successful, null if not.</returns>
        public static Artist ParseArtist(byte[] xml)
        {
            return Parse<Artist>(xml);
        }
        /// <summary>
        /// Parse <code>xml</code> into a <see cref="Album"/> object.
        /// </summary>
        /// <param name="xml">The xml as bytes.</param>
        /// <returns>A <see cref="Album"/> object if successful, null if not.</returns>
        public static Album ParseAlbum(byte[] xml)
        {
            return Parse<Album>(xml);
        }
        #endregion
    }
}
