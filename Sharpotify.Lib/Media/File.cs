using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharpotify.Util;

namespace Sharpotify.Media
{
    /// <summary>
    /// Holds information about a file.
    /// </summary>
    public class File : IComparable<File>
    {
        #region Consts
        public const int BITRATE_96  =  96000;
	    public const int BITRATE_160 = 160000;
        public const int BITRATE_320 = 320000;
        #endregion
        #region Fields
        /// <summary>
        /// The files 40-character hex identifier.
        /// </summary>
        private string _id;
        /// <summary>
        /// The files format. e.g. Ogg Vorbis,320000,...
        /// </summary>
        private string _format;
        #endregion
        #region Properties
        /// <summary>
        /// Get/Set the files identifier.
        /// </summary>
        public string Id
        {
            get
            {
                return this._id;
            }
            set
            {
                if (value == null || value.Length != 40 || !Hex.IsHex(value))
                    throw new ArgumentException("Expecting a 40-character hex string.");
                this._id = value;
            }
        }
        /// <summary>
        /// Get/Set the files format.
        /// </summary>
        public string Format { get { return this._format; } set { this._format = value; } }
        /// <summary>
        /// Get the files bitrate
        /// </summary>
        public int Bitrate
        {
            get 
            {
                return int.Parse(this._format.Split(',')[1]);
            }
        }
        #endregion
        #region Factory
        /// <summary>
        /// Creates an empty <see cref="File"/> object.
        /// </summary>
        protected File()
        {
            this._id = null;
            this._format = null;
        }
        /// <summary>
        /// Creates a <see cref="File"/> object with the specified <code>id</code> and <code>format</code>.
        /// </summary>
        /// <param name="id">Id of the file.</param>
        /// <param name="format">Format of the file.</param>
        public File(string id, string format)
        {
            /* Check if id string is valid. */
            if (id == null || id.Length != 40 || !Hex.IsHex(id))
                throw new ArgumentException("Expecting a 40-character hex string.");

            this._id = id;
            this._format = format;
        }
        #endregion
        #region Methods
        public int CompareTo(File f)
        {
            return this.Bitrate - f.Bitrate;
        }
        public override bool Equals(object obj)
        {
            if (obj is File)
            {
                File f = (obj as File);
                if (this._id.Equals(f.Id))
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
            return string.Format("[File: %s]", this._format);
        }
        #endregion
    }
}
