using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using Sharpotify.Util;
using Sharpotify.Enums;
using Sharpotify.Media;
using Sharpotify.Exceptions;

namespace Sharpotify
{
    public class SpotifyPool : ISpotify
    {
        #region Fields
        private List<ISpotify> connectionList;
        private BlockingQueue<ISpotify> connectionQueue;
        private int poolSize;
        private string userName;
        private string password;
        #endregion
        #region Singleton
        private static int connectionsNumber = 3;
        public static int MaxConnections { get { return connectionsNumber; } set { connectionsNumber = value; } }
        private static ISpotify _instance = null;
        public static ISpotify Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SpotifyPool();
                return _instance;
            }
        }
        #endregion
        #region Factory
        public SpotifyPool() : this(SpotifyPool.MaxConnections)
        {

        }
        public SpotifyPool(int size)
        {
            this.connectionList = new List<ISpotify>();
            this.connectionQueue = new BlockingQueue<ISpotify>(size);
            this.poolSize = size;
            this.userName = null;
            this.password = null;
        }
        #endregion
        #region Private Pool Methods
        [MethodImpl(MethodImplOptions.Synchronized)]
        private ISpotify CreateConnection()
        {
            /* Check if username and password are set. */
            if (this.userName == null || this.password == null)
            {
                throw new Exception("Not logged in!");
            }

            /* Create a new connection. */
            SpotifyConnection connection = new SpotifyConnection();

            /* Try to login with given username and password. */
            connection.Login(this.userName, this.password);

            /* Add connection to pool. */
            this.connectionList.Add(connection);

            return connection;
        }

        private void ReleaseConnection(ISpotify connection)
        {
            this.connectionQueue.Enqueue(connection);
        }

        private ISpotify GetConnection()
        {
            ISpotify connection;

            /* Check if pool size is reached. */
            if (this.connectionList.Count >= this.poolSize)
            {
                /* Try to get a connection from the queue. */
                try
                {
                    if (!this.connectionQueue.TryDequeue(out connection, new TimeSpan(0, 0, 10)))
                    {
                        throw new TimeoutException("Couldn't get connection after 10 seconds.");
                    }
                }
                catch (TimeoutException e)
                {
                    throw e;
                }
                catch (Exception e)
                {
                    throw e;
                }

            }
            else
            {
                /* Create a new connection. */
                try
                {
                    connection = this.CreateConnection();
                }
                catch (ConnectionException e)
                {
                    throw e;
                }
                catch (AuthenticationException e)
                {
                    throw e;
                }
            }

            return connection;
        }
        #endregion
        #region ISpotify Methods
        public Album Browse(Album album)
        {
            ISpotify connection = GetConnection();

            Album result = connection.Browse(album);

            ReleaseConnection(connection);

            return result;
        }
        public Artist Browse(Artist artist)
        {
            ISpotify connection = GetConnection();

            Artist result = connection.Browse(artist);

            ReleaseConnection(connection);

            return result;
        }
        public Track Browse(Track track)
        {
            ISpotify connection = GetConnection();

            Track result = connection.Browse(track);

            ReleaseConnection(connection);

            return result;
        }
        public List<Track> Browse(List<Track> tracks)
        {
            ISpotify connection = GetConnection();

            List<Track> result = connection.Browse(tracks);

            ReleaseConnection(connection);

            return result;
        }
        public Album BrowseAlbum(string id)
        {
            ISpotify connection = GetConnection();

            Album result = connection.BrowseAlbum(id);

            ReleaseConnection(connection);

            return result;
        }
        public Artist BrowseArtist(string id)
        {
            ISpotify connection = GetConnection();

            Artist result = connection.BrowseArtist(id);

            ReleaseConnection(connection);

            return result;
        }
        public Track BrowseTrack(string id)
        {
            ISpotify connection = GetConnection();

            Track result = connection.BrowseTrack(id);

            ReleaseConnection(connection);

            return result;
        }
        public List<Track> BrowseTracks(List<string> ids)
        {
            ISpotify connection = GetConnection();

            List<Track> result = connection.BrowseTracks(ids);

            ReleaseConnection(connection);

            return result;
        }
        public void Close()
        {
            this.connectionQueue.Close();

            /* Close all connections. */
            foreach (ISpotify connection in this.connectionList)
                connection.Close();

            this.connectionList.Clear();
        }

        public MusicStream GetMusicStream(Track track, File file, TimeSpan timeout)
        {
            ISpotify connection = GetConnection();

            MusicStream result = connection.GetMusicStream(track, file, timeout);

            ReleaseConnection(connection);

            return result;
        }
        public System.Drawing.Image Image(string id)
        {
            ISpotify connection = GetConnection();

            System.Drawing.Image result = connection.Image(id);

            ReleaseConnection(connection);

            return result;

        }
        public void Login(string username, string password)
        {
            if (this.connectionList.Count != 0)
            {
                throw new AuthenticationException("Already logged in!");
            }

            this.userName = username;
            this.password = password;

            ISpotify connection = CreateConnection();
            ReleaseConnection(connection);
        }
        public Playlist Playlist(string id)
        {
            ISpotify connection = GetConnection();

            Playlist result = connection.Playlist(id);

            ReleaseConnection(connection);

            return result;
        }
        public Playlist Playlist(string id, bool cached)
        {
            ISpotify connection = GetConnection();

            Playlist result = connection.Playlist(id, cached);

            ReleaseConnection(connection);

            return result;
        }
        public PlaylistContainer PlaylistContainer()
        {
            ISpotify connection = GetConnection();

            PlaylistContainer result = connection.PlaylistContainer();

            ReleaseConnection(connection);

            return result;
        }
        public bool PlaylistContainerAddPlaylist(PlaylistContainer playlistContainer, Playlist playlist)
        {
            ISpotify connection = GetConnection();

            bool result = connection.PlaylistContainerAddPlaylist(playlistContainer, playlist);

            ReleaseConnection(connection);

            return result;
        }
        public bool PlaylistContainerAddPlaylist(PlaylistContainer playlistContainer, Playlist playlist, int position)
        {
            ISpotify connection = GetConnection();

            bool result = connection.PlaylistContainerAddPlaylist(playlistContainer, playlist, position);

            ReleaseConnection(connection);

            return result;
        }
        public bool PlaylistContainerAddPlaylists(PlaylistContainer playlistContainer, List<Playlist> playlists, int position)
        {
            ISpotify connection = GetConnection();

            bool result = connection.PlaylistContainerAddPlaylists(playlistContainer, playlists, position);

            ReleaseConnection(connection);

            return result;
        }
        public bool PlaylistContainerRemovePlaylist(PlaylistContainer playlistContainer, int position)
        {
            ISpotify connection = GetConnection();

            bool result = connection.PlaylistContainerRemovePlaylist(playlistContainer, position);

            ReleaseConnection(connection);

            return result;
        }
        public bool PlaylistContainerRemovePlaylists(PlaylistContainer playlistContainer, int position, int count)
        {
            ISpotify connection = GetConnection();

            bool result = connection.PlaylistContainerRemovePlaylists(playlistContainer, position, count);

            ReleaseConnection(connection);

            return result;
        }
        public Playlist PlaylistCreate(Album sourceAlbum)
        {
            ISpotify connection = GetConnection();

            Playlist result = connection.PlaylistCreate(sourceAlbum);

            ReleaseConnection(connection);

            return result;
        }
        public Playlist PlaylistCreate(string name, bool collaborative, string description, string picture)
        {
            ISpotify connection = GetConnection();

            Playlist result = connection.PlaylistCreate(name, collaborative, description, picture);

            ReleaseConnection(connection);

            return result;
        }
        public Playlist PlaylistCreate(string name)
        {
            ISpotify connection = GetConnection();

            Playlist result = connection.PlaylistCreate(name);

            ReleaseConnection(connection);

            return result;
        }
        public bool PlaylistDestroy(Playlist playlist)
        {
            ISpotify connection = GetConnection();

            bool result = connection.PlaylistDestroy(playlist);

            ReleaseConnection(connection);

            return result;
        }
        public bool PlaylistAddTrack(Playlist playlist, Track track)
        {
            ISpotify connection = GetConnection();

            bool result = connection.PlaylistAddTrack(playlist, track);

            ReleaseConnection(connection);

            return result;
        }
        public bool PlaylistAddTrack(Playlist playlist, Track track, int position)
        {
            ISpotify connection = GetConnection();

            bool result = connection.PlaylistAddTrack(playlist, track, position);

            ReleaseConnection(connection);

            return result;
        }
        public bool PlaylistAddTracks(Playlist playlist, List<Track> tracks, int position)
        {
            ISpotify connection = GetConnection();

            bool result = connection.PlaylistAddTracks(playlist, tracks, position);

            ReleaseConnection(connection);

            return result;
        }
        public bool PlaylistMoveTrack(Playlist playlist, int sourcePosition, int destPosition)
        {
            ISpotify connection = GetConnection();

            bool result = connection.PlaylistMoveTrack(playlist, sourcePosition, destPosition);

            ReleaseConnection(connection);

            return result;
        }
        public bool PlaylistRemoveTrack(Playlist playlist, int position)
        {
            ISpotify connection = GetConnection();

            bool result = connection.PlaylistRemoveTrack(playlist, position);

            ReleaseConnection(connection);

            return result;
        }
        public bool PlaylistRemoveTracks(Playlist playlist, int position, int count)
        {
            ISpotify connection = GetConnection();

            bool result = connection.PlaylistRemoveTracks(playlist, position, count);

            ReleaseConnection(connection);

            return result;
        }
        public bool PlaylistRename(Playlist playlist, string name)
        {
            ISpotify connection = GetConnection();

            bool result = connection.PlaylistRename(playlist, name);

            ReleaseConnection(connection);

            return result;
        }
        public bool PlaylistSetCollaborative(Playlist playlist, bool collaborative)
        {
            ISpotify connection = GetConnection();

            bool result = connection.PlaylistSetCollaborative(playlist, collaborative);

            ReleaseConnection(connection);

            return result;
        }
        public bool PlaylistSetInformation(Playlist playlist, string description, string picture)
        {
            ISpotify connection = GetConnection();

            bool result = connection.PlaylistSetInformation(playlist, description, picture);

            ReleaseConnection(connection);

            return result;
        }
        public Track Replacement(Track track)
        {
            ISpotify connection = GetConnection();

            Track result = connection.Replacement(track);

            ReleaseConnection(connection);

            return result;

        }
        public List<Track> Replacement(List<Track> tracks)
        {
            ISpotify connection = GetConnection();

            List<Track> result = connection.Replacement(tracks);

            ReleaseConnection(connection);

            return result;

        }
        public Result Search(string query)
        {
            ISpotify connection = GetConnection();

            Result result = connection.Search(query);

            ReleaseConnection(connection);

            return result;

        }
        public Result Toplist(Sharpotify.Enums.ToplistType type, string region, string username)
        {
            ISpotify connection = GetConnection();

            Result result = connection.Toplist(type, region, username);

            ReleaseConnection(connection);

            return result;

        }
        public User User()
        {
            ISpotify connection = GetConnection();

            User result = connection.User();

            ReleaseConnection(connection);

            return result;
        }
        #endregion
    }
}
