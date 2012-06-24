using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Media
{
    /// <summary>
    /// Holds information about a disc of an album.
    /// </summary>
    public class Disc
    {
        #region Fields
        /// <summary>
        /// The number of this disc.
        /// </summary>
        private int _number;
        /// <summary>
        /// The name of this disc.
        /// </summary>
        private string _name;
        /// <summary>
        /// A list of tracks on this disc.
        /// </summary>
        private List<Track> _tracks;
        #endregion
        #region Properties
        /// <summary>
        /// Get/Set the discs number.
        /// </summary>
        public int Number { get { return this._number; } set { this._number = value; } }
        /// <summary>
        /// Get/Set the discs name.
        /// </summary>
        public string Name { get { return this._name; } set { this._name = value; } }
        /// <summary>
        /// Get/Set the list of tracks on this disc.
        /// </summary>
        public List<Track> Tracks { get { return this._tracks; } set { this._tracks = value; } }
        #endregion
        #region Factory
        /// <summary>
        /// Create an empty <see cref="Disc"/> object.
        /// </summary>
        public Disc()
        {
            this.Number = -1;
            this.Name = null;
            this.Tracks = new List<Track>();
        }
        /// <summary>
        /// Create a <see cref="Disc"/> object with the specified <code>number</code> and <code>name</code>.
        /// </summary>
        /// <param name="number">The discs number.</param>
        /// <param name="name">The discs name.</param>
        public Disc(int number, string name)
        {
            this.Number = number;
            this.Name = name;
            this.Tracks = new List<Track>();
        }
        #endregion
    }
}
