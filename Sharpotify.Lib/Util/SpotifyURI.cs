using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sharpotify.Exceptions;
using Sharpotify.Enums;
using Sharpotify.Util;

namespace Sharpotify.Util
{
    internal class SpotifyURI
    {
        #region fields

        private SpotifyURIType _type;
        private string _id;

        #endregion

        #region properties

        public SpotifyURIType Type
        {
            get { return _type; }
        }

        public string Id
        {
            get { return _id; }
        }

        public bool IsAlbumURI { get { return Type == SpotifyURIType.ALBUM; } }
        public bool IsArtistURI { get { return Type == SpotifyURIType.ARTIST; } }
        public bool IsTrackURI { get { return Type == SpotifyURIType.TRACK; } }

        #endregion

        #region methods

        public override string ToString()
        {
            return string.Format("spotify:{0}:{1}", EnumUtils.GetName(typeof(SpotifyURIType), this.Type).ToLower(), this.Id);
        }

        public static string ToHex(string base62)
        {
            string hex = BaseConvert.Convert(base62, 62, 16);
            if (hex.Length >= 32)
                return hex;
            else
                return new string('0', 32 - hex.Length) + hex;
        }

        public static string ToBase62(string hex)
        {
            string base62 = BaseConvert.Convert(hex, 16, 62);
            if (base62.Length >= 22)
                return base62;
            else
                return new string('0', 22 - base62.Length) + base62;
        }

        #endregion

        #region construction

        public SpotifyURI(string uri)
        {
            try
            {
                Match regexpMatch = Regex.Match(uri, "spotify:(artist|album|track):([0-9A-Za-z]{22})");
                if (regexpMatch.Success)
                {
                    string type = regexpMatch.Groups[1].Value;
                    this._type = (SpotifyURIType)Enum.Parse(typeof(SpotifyURIType), type, true);
                    this._id = regexpMatch.Groups[2].Value;
                }
                else
                    throw new InvalidSpotifyURIException();
            }
            catch (Exception ex)
            {
                throw new InvalidSpotifyURIException(ex);
            }
        }

        #endregion
    }
}
