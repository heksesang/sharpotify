using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sharpotify.Media.Parser
{
    public class XMLPlaylistParser : XMLParser
    {
        #region Factory
        /// <summary>
        /// Create a new stream parser from the given input stream.
        /// </summary>
        /// <param name="stream">An stream to parse.</param>
        private XMLPlaylistParser(Stream stream) : base(stream)
        {

        }
        #endregion
        #region Private Methods
        /// <summary>
        /// Parse the input stream as one of <see cref="PlaylistContainer"/> or
        /// <see cref="Playlist"/>, depending on the document element.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private object Parse(string id)
        {
            string name;

            /* Check if reader is currently on a start element. */
            if (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

                /* Check current element name and start parsing it. */
                if (name.Equals("playlists"))
                {
                    if (string.IsNullOrEmpty(id))
                        return this.ParsePlaylistContainer();
                    else
                        return this.ParsePlaylist(id);
                }
                else if (name.Equals("playlist"))
                {
                    return this.ParsePlaylist(id);
                }
                else if (name.Equals("confirm"))
                {
                    return this.ParsePlaylistConfirmation();
                }
                else
                {
                    throw new XMLParserException("Unexpected element '<" + name + ">'");
                }
            }

            throw new XMLParserException("Reader is not on a start element!");
        }
        private PlaylistContainer ParsePlaylistContainer()
        {
            PlaylistContainer playlists = new PlaylistContainer();
            string name;

            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

                if (name.Equals("base-version"))
                {
                    this.SkipBaseVersion();
                }
                else if (name.Equals("next-change"))
                {
                    this.ParseNextChange(playlists);
                }
                else
                    throw new XMLParserException("Unexpected element '<" + name + ">'");

                this.Next();
            }

            return playlists;
        }
        private Playlist ParsePlaylist(string id)
        {
            Playlist playlist = new Playlist();
            string name;

            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

                if (name.Equals("base-version"))
                {
                    this.SkipBaseVersion();
                }
                else if (name.Equals("next-change"))
                {
                    this.ParseNextChange(playlist);
                }
                else
                    throw new XMLParserException("Unexpected element '<" + name + ">'");

                this.Next();
            }

            playlist.Id = id;

            return playlist;
        }
        private void SkipBaseVersion()
        {
            string name;

            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

                if (name.Equals("rid"))
                {
                    this.GetElementString(); /* Skip. */
                }
                else if (name.Equals("version"))
                {
                    this.GetElementString(); /* Skip. */
                }
                else
                    throw new XMLParserException("Unexpected element '<" + name + ">'");

                this.Next();
            }
        }
        private void ParseNextChange(object obj)
        { 
            string name;

		    /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

			    if(name.Equals("change")){
				    this.ParseChange(obj);
			    }
			    else if(name.Equals("rid")){
				    this.GetElementString(); /* Skip. */
			    }
			    else if(name.Equals("version")){
				    string[] parts = this.GetElementString().Split(',');

				    if(obj is Playlist)
                    {
					    Playlist playlist = (Playlist)obj;

					    playlist.Revision = long.Parse(parts[0]);
					    playlist.Checksum = long.Parse(parts[2]);
					    playlist.IsCollaborative = (int.Parse(parts[3]) == 1);
				    }
				    else if(obj is PlaylistContainer)
                    {
					    PlaylistContainer playlists = (PlaylistContainer)obj;

					    playlists.Revision = long.Parse(parts[0]);
					    playlists.Checksum = long.Parse(parts[2]);
				    }
				    else
                        throw new XMLParserException("Unexpected object '" + obj + "'");
			    }
			     else
                    throw new XMLParserException("Unexpected element '<" + name + ">'");

                this.Next();
		    }
        }
        private void ParseChange(object obj)
        { 
            string name;

		    /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;
		
			    if(name.Equals("ops")){
                    this.ParseOps(obj);
			    }
			    else if(name.Equals("time")){
				    this.GetElementString(); /* Skip. */
			    }
			    else if(name.Equals("user"))
                {
				    if(obj is Playlist){
					    ((Playlist)obj).Author = this.GetElementString();
				    }
				    else if(obj is PlaylistContainer){
					    ((PlaylistContainer)obj).Author = this.GetElementString();
				    }
				    else
					    throw new XMLParserException("Unexpected object '" + obj + "'");

				    /* Skip characters. */
				    //this.Next();
			    }
			    else
                    throw new XMLParserException("Unexpected element '<" + name + ">'");

                this.Next();
		    }
        }
        private void ParseOps(object obj)
        { 
             string name;

		    /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

			    if(name.Equals("create")){
				    this.GetElementString(); /* Skip. */
			    }
			    else if(name.Equals("pub") && obj is Playlist){
				    ((Playlist)obj).IsCollaborative = (this.GetElementInteger() == 1);
			    }
			    else if(name.Equals("name") && obj is Playlist){
				    ((Playlist)obj).Name = this.GetElementString();
			    }
			    else if(name.Equals("description") && obj is Playlist){
				    ((Playlist)obj).Description = this.GetElementString();
			    }
			    else if(name.Equals("picture") && obj is Playlist){
				    ((Playlist)obj).Picture = this.GetElementString();
			    }
			    else if(name.Equals("add"))
                {
                    this.Next();
                    while (this.reader.IsStartElement())
                    {
                        name = this.reader.LocalName;

					    if(name.Equals("i")){
						    this.GetElementString(); /* Skip. */
					    }
					    else if(name.Equals("items")){
						    string[] lines = this.GetElementString().Split(new char[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries);
						    if(obj is Playlist)
                            {
							    List<Track> tracks = new List<Track>();

                                foreach(string track in lines)
								    tracks.Add(new Track(track.Trim().Substring(0, 32)));

							    ((Playlist)obj).Tracks = tracks;
						    }
						    else if(obj is PlaylistContainer)
                            {
							    List<Playlist> playlists = new List<Playlist>();

							    foreach(string playlist in lines)
								    playlists.Add(new Playlist(playlist.Substring(0, 32)));

							    ((PlaylistContainer)obj).Playlists = playlists;
						    }
						    else
							    throw new XMLParserException("Unexpected object '" + obj + "'");
					    }
					    else
                            throw new XMLParserException("Unexpected element '<" + name + ">'");

                        this.Next();
				    }
			    }
			    else if(name.Equals("set-attribute")){
                    this.Next();
                    while (this.reader.IsStartElement())
                    {
                        name = this.reader.LocalName;

					    if(name.Equals("i")){
						    this.GetElementString(); /* Skip. */
					    }
					    else if(name.Equals("key")){
                            this.GetElementString(); /* Skip. */
					    }
					    else if(name.Equals("value")){
                            this.GetElementString(); /* Skip. */
					    }
                        else
                            throw new XMLParserException("Unexpected element '<" + name + ">'");

                        this.Next();
				    }
			    }
			    else
                    throw new XMLParserException("Unexpected element '<" + name + ">'");

                this.Next();
		    }
        }
        private PlaylistConfirmation ParsePlaylistConfirmation()
        {
            PlaylistConfirmation confirmation = new PlaylistConfirmation();
            string name;

            /* Go to next element and check if it is a start element. */
            this.Next();
            while (this.reader.IsStartElement())
            {
                name = this.reader.LocalName;

                if (name.Equals("rid"))
                {
                    this.GetElementString(); /* Skip. */
                }
                else if (name.Equals("version"))
                {
                    string[] parts = this.GetElementString().Split(',');

                    confirmation.Revision = long.Parse(parts[0]);
                    confirmation.Checksum = long.Parse(parts[2]);
                    confirmation.Collaborative = (int.Parse(parts[3]) == 1);
                }
                else
                    throw new XMLParserException("Unexpected element '<" + name + ">'");

                this.Next();
            }

            return confirmation;
        }
        #endregion
        #region Public Methods
        /// <summary>
        /// Parse <code>xml</code> into an object.
        /// </summary>
        /// <param name="data">The xml as bytes.</param>
        /// <returns>An object if successful, null if not.</returns>
        public static object Parse(byte[] data, string id)
        {
            try
            {
                XMLPlaylistParser parser = new XMLPlaylistParser(new MemoryStream(data));

                return parser.Parse(id);
            }
            catch (XMLParserException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// Parse <code>xml</code> into a <see cref="PlaylistContainer"/> object.
        /// </summary>
        /// <param name="data">The xml as bytes.</param>
        /// <returns><see cref="PlaylistContainer"/> object if successful, null if not.</returns>
        public static PlaylistContainer ParsePlaylistContainer(byte[] data)
        {
		    /* Wrap xml data in corrent document element. */
		    object playlistContainer = Parse(
                Encoding.UTF8.GetBytes(
			        ("<?xml version=\"1.0\" encoding=\"utf-8\" ?><playlists>" +
				        Encoding.UTF8.GetString(data) +
			        "</playlists>")),
			    null
		    );

		    if(playlistContainer is PlaylistContainer)
            {
			    return (PlaylistContainer)playlistContainer;
		    }

		    return null;
	    }
        /// <summary>
        /// Parse <code>xml</code> into a <see cref="Playlist"/> object.
        /// </summary>
        /// <param name="data">The xml as bytes.</param>
        /// <returns><see cref="Playlist"/> object if successful, null if not.</returns>
        public static Playlist ParsePlaylist(byte[] data, string id)
        {
            /* Wrap xml data in corrent document element. */
            object playlist = Parse(
                Encoding.UTF8.GetBytes(
                    ("<?xml version=\"1.0\" encoding=\"utf-8\" ?><playlists>" +
                        Encoding.UTF8.GetString(data) +
                    "</playlists>")),
                id
            );

            if (playlist is Playlist)
            {
                return (Playlist)playlist;
            }

            return null;
        }
        /// <summary>
        /// Parse <code>xml</code> into a <see cref="PlaylistConfirmation"/> object.
        /// </summary>
        /// <param name="data">The xml as bytes.</param>
        /// <returns><see cref="PlaylistConfirmation"/> object if successful, null if not.</returns>
        public static PlaylistConfirmation ParsePlaylistConfirmation(byte[] data)
        {
            /* Wrap xml data in corrent document element. */
            object playlist = Parse(
                Encoding.UTF8.GetBytes(
                    "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" +
                        Encoding.UTF8.GetString(data)),
                null
            );

            if (playlist is PlaylistConfirmation)
            {
                return (PlaylistConfirmation)playlist;
            }

            return null;
        }
        #endregion
    }
}
