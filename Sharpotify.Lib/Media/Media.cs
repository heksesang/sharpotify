using System;
using System.Collections.Generic;
using System.Text;

using Sharpotify.Util;

namespace Sharpotify.Media
{
    /// <summary>
    /// Holds basic information about media.
    /// </summary>
    public class Media
    {
        #region Fields
        /// <summary>
        /// Identifier for this media object (32-character hex string).
        /// </summary>
        protected string _id = null;
        /// <summary>
        /// Redirects (other identifiers) for this media (32-character hex strings).
        /// </summary>
        protected List<string> _redirects;
        /// <summary>
        /// Popularity of this media (from 0.0 to 1.0).
        /// </summary>
        private Single _popularity;
        /// <summary>
        /// Restrictions of this media.
        /// </summary>
        private List<Restriction> _restrictions;
        /// <summary>
        /// External ids of this media.
        /// </summary>
        private Dictionary<string, string> _externalIds;
        #endregion
        #region Properties
        /// <summary>
        /// Get/Set the media identifier.
        /// </summary>
        public string Id
        {
            get { return this._id; }
            set
            {
                if (value == null || value.Length != 32 || !Hex.IsHex(value))
                    throw new ArgumentException("Expecting a 32-character hex string.");
                this._id = value;
            }
        }

        /// <summary>
        /// Get the media redirects.
        /// </summary>
        public List<string> Redirects
        {
            get
            {
                return _redirects;
            }
        }
        /// <summary>
        /// Get/Set the media popularity.
        /// <remarks>A decimal value between 0.0 and 1.0 or <see cref="float.NaN"/> if not available.</remarks>
        /// </summary>
        public float Popularity 
        { 
            get 
            { 
                return _popularity; 
            }
            set
            {
                if (value != float.NaN && (value < 0.0 || value > 1.0))
                    throw new ArgumentException("Expecting a value from 0.0 to 1.0 or Float.NAN.");
                this._popularity = value;
            }
        }
        /// <summary>
        /// Get/Set the media restrictions.
        /// </summary>
        public List<Restriction> Restrictions
        {
            get { return _restrictions; }
            set { _restrictions = value; }
        }

        /// <summary>
        /// Get/Set the media external identifiers.
        /// </summary>
        public Dictionary<string, string> ExternalIds
        {
            get { return _externalIds; }
            set { _externalIds = value; }
        }
        #endregion
        #region Factory
        /// <summary>
        /// Creates an empty <see cref="Media"/> object.
        /// </summary>
        protected Media()
        {
            this._id = null;
            this._redirects = new List<string>();
            this._popularity = Single.NaN;
            this._restrictions = new List<Restriction>();
            this._externalIds = new Dictionary<string, string>();
        }
        /// <summary>
        ///  Creates a <see cref="Media"/> object with the specified id.
        /// </summary>
        /// <param name="id">A 32-character hex string or a Spotify URI.</param>
        protected Media(string id)
            : this()
        {
            /* Check if id is a 32-character hex string. */
            if (id.Length == 32 && Hex.IsHex(id))
            {
                this._id = id;
            }
            /* Otherwise try to parse it as a Spotify URI. */
            else
            {
                try
                {
                    this._id = Link.Create(id).Id;
                }
                catch (Link.InvalidSpotifyURIException e)
                {
                    throw new ArgumentException(
                        "Given id is neither a 32-character " +
                        "hex string nor a valid Spotify URI.", e
                    );
                }
            }
        }
        #endregion
        #region Methods
        /// <summary>
        /// Add a media redirect.
        /// </summary>
        /// <param name="redirect">A 32-character identifier.</param>
        public void AddRedirect(string redirect)
        {
            _redirects.Add(redirect);
        }
        /// <summary>
        /// Check if the media is restricted for the given {@code country} and {@code catalogue}.
        /// </summary>
        /// <param name="country">A 2-letter country code.</param>
        /// <param name="catalogue">The catalogue to check.</param>
        /// <returns>true if it is restricted, false otherwise.</returns>
        public bool IsRestricted(string country, string catalogue)
        {
		    if(country.Length != 2)
			    throw new ArgumentException("Expecting a 2-letter country code.");
		    
    		foreach (Restriction restriction in this._restrictions)
            {
			    if(restriction.IsCatalogue(catalogue) 
                    && (restriction.IsForbidden(country) || !restriction.IsAllowed(country)))
				    return true;
		    }
		    return false;
	    }
        /// <summary>
        /// Get an external identifier for the specified {@code service}.
        /// </summary>
        /// <param name="service">The service to get the identifer for.</param>
        /// <returns>An identifier or null if not available.</returns>
        public string GetExternalId(string service)
        {
            if (_externalIds.ContainsKey(service))
                return _externalIds[service];
            return null;
        }
        #endregion
    }
}
