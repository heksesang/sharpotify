using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Media
{
    public class Biography
    {
        #region Fields
        /// <summary>
        /// A Biographical text.
        /// </summary>
        private string _text;
        /// <summary>
        /// A list of portrait image ids.
        /// </summary>
        private List<string> _portraits;
        #endregion
        #region Properties
        /// <summary>
        /// Get/Set the biographical text.
        /// </summary>
        public string Text { get { return this._text; } set { this._text = value; } }
        /// <summary>
        /// Get/Set portraits for this biography.
        /// </summary>
        public List<string> Portraits { get { return this._portraits; } set { this._portraits = value; } }
        #endregion
        #region Factory
        /// <summary>
        /// Creates an empty <see cref="Biography"/> object.
        /// </summary>
        public Biography() : this(null)
        {
            
        }
        /// <summary>
        /// Creates a <see cref="Biography"/> object with the specified <code>text</code>.
        /// </summary>
        /// <param name="text">A Biographical text.</param>
        public Biography(string text)
        {
            this.Text = text;
            this.Portraits = new List<string>();
        }
        #endregion
    }
}
