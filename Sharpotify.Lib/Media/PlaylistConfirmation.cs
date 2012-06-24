using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Media
{
    public class PlaylistConfirmation
    {
        #region Fields
        private long _revision;
        private long _checksum;
        private bool _collaborative;
        #endregion
        #region Properties
        public long Revision { get { return this._revision; } set { this._revision = value; } }
        public long Checksum { get { return this._checksum; } set { this._checksum = value; } }
        public bool Collaborative { get { return this._collaborative; } set { this._collaborative = value; } }
        #endregion
        #region Factory
        public PlaylistConfirmation()
        {
            this._revision = -1;
            this._checksum = -1;
            this._collaborative = false;
        }
        #endregion
    }
}
