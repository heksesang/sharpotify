using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Sharpotify.Media
{
    /// <summary>
    /// Holds restriction information of media. Every operation in this class
    /// automatically converts strings to lower-case before comparing with them.
    /// </summary>
    public class Restriction
    {
        #region Fields
        /// <summary>
        /// A delimeter separated list of allowed 2-letter country codes.
        /// </summary>
        private List<RegionInfo> _allowed;
        /// <summary>
        /// A delimeter separated list of forbidden 2-letter country codes.
        /// </summary>
        private List<RegionInfo> _forbidden;
        /// <summary>
        /// A delimeter separated list of catalogues this restriction applies to.
        /// </summary>
        private List<string> _catalogues;
        /// <summary>
        /// The type of content this restriction applies to.
        /// </summary>
        private string _type;
        #endregion
        #region Properties
        /// <summary>
        /// Get/Set a delimeter separated list of allowed 2-letter country codes.
        /// </summary>
        public List<RegionInfo> Allowed
        {
            get
            {
                return this._allowed;
            }
            set
            {
                this._allowed = value;
            }
        }
        /// <summary>
        /// Get/Set a delimeter separated list of forbidden 2-letter country codes.
        /// </summary>
        public List<RegionInfo> Forbidden
        {
            get
            {
                return this._forbidden;
            }
            set
            {
                this._forbidden = value;
            }
        }
        /// <summary>
        /// Get/Set a delimeter separated list of catalogues this restriction applies to.
        /// </summary>
        public List<string> Catalogues
        {
            get
            {
                return this._catalogues;
            }
            set
            {
                this._catalogues = value;
            }
        }
        /// <summary>
        /// Get/Set the type of content this restriction applies to.
        /// </summary>
        public string Type
        {
            get
            {
                return this._type;
            }
            set
            {
                this._type = value;
            }
        }
        #endregion
        #region Factory
        /// <summary>
        /// Create an empty <see cref="Restriction"/> object.
        /// </summary>
        public Restriction()
        {
            this._allowed = new List<RegionInfo>();
            this._forbidden = new List<RegionInfo>();
            this._catalogues = new List<string>();
            this._type = string.Empty;
        }
        /// <summary>
        /// Create a <see cref="Restriction"/> object with the specified countries and catalogues.
        /// A delimeter can be a comma or a space for example.
        /// </summary>
        /// <param name="allowed">A delimeter separated list of allowed 2-letter country codes.</param>
        /// <param name="forbidden">A delimeter separated list of forbidden 2-letter country codes.</param>
        /// <param name="catalogues">A delimeter separated list of catalogues this restriction applies to.</param>
        /// <param name="type">The type of content this restriction applies to.</param>
        public Restriction(List<RegionInfo> allowed, List<RegionInfo> forbidden, List<string> catalogues, string type)
        {
            this.Allowed = allowed;
            this.Forbidden = forbidden;
            this.Catalogues = catalogues;
            this.Type = type;
        }
        #endregion
        #region Methods
        /// <summary>
        /// Check if a country is allowed by this restriction.
        /// </summary>
        /// <param name="country">A 2-letter country code.</param>
        /// <returns>true if it is allowed, false otherwise.</returns>
        public bool IsAllowed(string country)
        {
            if (country.Length != 2)
                throw new ArgumentException("Expecting a 2-letter country code.");
            return this.Allowed != null && this.Allowed.Any(r => r.TwoLetterISORegionName.ToLowerInvariant() == country.ToLowerInvariant());
        }
        /// <summary>
        /// Check if a country is forbidden by this restriction.
        /// </summary>
        /// <param name="country">A 2-letter country code.</param>
        /// <returns>true if it is allowed, false otherwise.</returns>
        public bool IsForbidden(string country)
        {
            if (country.Length != 2)
                throw new ArgumentException("Expecting a 2-letter country code.");
            return this.Forbidden != null && this.Forbidden.Any(r => r.TwoLetterISORegionName.ToLowerInvariant() == country.ToLowerInvariant());
        }
        /// <summary>
        /// Check if this restriction applies to a specified catalogue.
        /// </summary>
        /// <param name="catalogue">A catalogue to test.</param>
        /// <returns>true if it applies, false otherwise.</returns>
        public bool IsCatalogue(string catalogue)
        {
            return this.Catalogues != null && this.Catalogues.Any(c => c.ToLowerInvariant() == catalogue.ToLowerInvariant());
        }

        public override string ToString()
        {
            return string.Format("[Restriction: %s, %s, %s]", this._catalogues, this._allowed, this._forbidden); ;
        }
        #endregion
    }
}
