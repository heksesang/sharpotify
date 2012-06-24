using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Cache
{
    public class NoCache : ICache
    {
        #region fields

        #endregion


        #region methods

        #endregion


        #region Cache members

        public void Clear()
        {
        }

        public void Clear(string category)
        {
        }

        public bool Contains(string category, string hash)
        {
            return false;
        }

        public byte[] Load(string category, string hash)
        {
            return null;
        }

        public void Remove(string category, string hash)
        {
        }

        public void Store(string category, string hash, byte[] data)
        {
        }

        public void Store(string category, string hash, byte[] data, int size)
        {
        }

        public string[] List(string category)
        {
            return new string[0];
        }

        #endregion


        #region construction

        public NoCache()
        {

        }

        #endregion
    }
}
