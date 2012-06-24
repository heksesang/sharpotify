using System;
using System.Collections.Generic;
using Sharpotify.Media;

namespace Sharpotify
{
    public interface ISpotify
    {
        Album Browse(Album album);
        Artist Browse(Artist artist);
        Track Browse(Track track);
        List<Track> Browse(List<Track> tracks);
        Album BrowseAlbum(string id);
        Artist BrowseArtist(string id);
        Track BrowseTrack(string id);
        List<Track> BrowseTracks(List<string> ids);
        void Close();
        MusicStream GetMusicStream(Track track, File file, TimeSpan timeout);
        System.Drawing.Image Image(string id);
        void Login(string username, string password);
        Playlist Playlist(string id);
        Playlist Playlist(string id, bool cached);
        PlaylistContainer PlaylistContainer();
        bool PlaylistContainerAddPlaylist(PlaylistContainer playlistContainer, Playlist playlist);
        bool PlaylistContainerAddPlaylist(PlaylistContainer playlistContainer, Playlist playlist, int position);
        bool PlaylistContainerAddPlaylists(PlaylistContainer playlistContainer, List<Playlist> playlists, int position);
        bool PlaylistContainerRemovePlaylist(PlaylistContainer playlistContainer, int position);
        bool PlaylistContainerRemovePlaylists(PlaylistContainer playlistContainer, int position, int count);
        Playlist PlaylistCreate(Album sourceAlbum);
        Playlist PlaylistCreate(string name, bool collaborative, string description, string picture);
        Playlist PlaylistCreate(string name);
        bool PlaylistDestroy(Playlist playlist);
        bool PlaylistAddTrack(Playlist playlist, Track track);
        bool PlaylistAddTrack(Playlist playlist, Track track, int position);
        bool PlaylistAddTracks(Playlist playlist, List<Track> tracks, int position);
        bool PlaylistRemoveTrack(Playlist playlist, int position);
        bool PlaylistRemoveTracks(Playlist playlist, int position, int count);
        bool PlaylistMoveTrack(Playlist playlist, int sourcePosition, int destPosition);
        bool PlaylistRename(Playlist playlist, string name);
        bool PlaylistSetCollaborative(Playlist playlist, bool collaborative);
        bool PlaylistSetInformation(Playlist playlist, string description, string picture);
        Track Replacement(Track track);
        List<Track> Replacement(List<Track> tracks);
        Result Search(string query);
        Result Toplist(Sharpotify.Enums.ToplistType type, string region, string username);
        User User();
    }
}
