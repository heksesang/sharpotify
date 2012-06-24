using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Cache
{
    public class MemoryCache : ICache
    {
        #region fields

        private Dictionary<string, Dictionary<string, byte[]>> _data = new Dictionary<string, Dictionary<string, byte[]>>();

        #endregion


        #region methods

        #endregion


        #region Cache members

        public void Clear()
        {
            _data.Clear();
        }

        public void Clear(string category)
        {
            _data[category].Clear();
        }

        public bool Contains(string category, string hash)
        {
            if (!_data.ContainsKey(category))
                return false;
            return _data[category].ContainsKey(hash);
        }

        public byte[] Load(string category, string hash)
        {
            try
            {
                return _data[category][hash];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Remove(string category, string hash)
        {
            try
            {
                _data[category].Remove(hash);
            }
            catch (Exception)
            {
                throw; //FIXME
            }
        }

        public void Store(string category, string hash, byte[] data)
        {
            Store(category, hash, data, data.Length);
        }

        public void Store(string category, string hash, byte[] data, int size)
        {
            try
            {
                byte[] buffer;
                if (data.Length == size)
                    buffer = data;
                else
                {
                    buffer = new byte[size];
                    Array.Copy(data, buffer, size);
                }

                if (!_data.ContainsKey(category))
                    _data.Add(category, new Dictionary<string, byte[]>());

                _data[category].Add(hash, buffer);
            }
            catch (Exception)
            {
                //Ignore errors
            }
        }

        public string[] List(string category)
        {
            try
            {
                return _data[category].Keys.ToArray();
            }
            catch (Exception)
            {
                throw; //FIXME
            }
        }

        #endregion


        #region construction

        public MemoryCache()
        {

        }

        #endregion
    }
}
