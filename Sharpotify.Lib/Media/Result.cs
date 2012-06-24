using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharpotify.Util;

namespace Sharpotify.Media
{
    public class Result
    {
        #region Fields
        private string _query;
        private string _suggestion;
        private int _totalArtists;
        private int _totalAlbums;
        private int _totalTracks;
        private List<Artist> _artists;
        private List<Album> _albums;
        private List<Track> _tracks;
        #endregion
        #region Properties
        public List<Album> Albums
        {
            get { return this._albums; }
            set { this._albums = value; }
        }

        public List<Artist> Artists
        {
            get { return this._artists; }
            set { this._artists = value; }
        }

        public string Query
        {
            get { return this._query; }
            set { this._query = value; }
        }

        public string Suggestion
        {
            get { return this._suggestion; }
            set { this._suggestion = value; }
        }

        public int TotalAlbums
        {
            get { return this._totalAlbums; }
            set { this._totalAlbums = value; }
        }

        public int TotalArtists
        {
            get { return this._totalArtists; }
            set { this._totalArtists = value; }
        }

        public int TotalTracks
        {
            get { return this._totalTracks; }
            set { this._totalTracks = value; }
        }

        public List<Track> Tracks
        {
            get { return this._tracks; }
            set { this._tracks = value; }
        }
        #endregion
        #region Factory
        public Result()
        {
            this._query = null;
            this._suggestion = null;
            this._totalArtists = 0;
            this._totalAlbums = 0;
            this._totalTracks = 0;
            this._artists = new List<Artist>();
            this._albums = new List<Album>();
            this._tracks = new List<Track>();
        }
        #endregion
        #region Methods
        public void AddAlbum(Album album)
        {
            this.Albums.Add(album);
        }

        public void AddArtist(Artist artist)
        {
            this.Artists.Add(artist);
        }

        public void AddTrack(Track track)
        {
            this.Tracks.Add(track);
        }

        public override bool Equals(object obj)
        {
            if (obj is Result) 
                return this._query.Equals((obj as Result).Query);
            return false;
        }

        public override int GetHashCode()
        {
            return ((this._query != null) ? this._query.GetHashCode() : 0);
        }
        #endregion
    }

     

}
