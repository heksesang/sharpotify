using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sharpotify.Cache
{
    /// <summary>
    /// A <see cref="ICache"/> implementation that stores data in the filesystem.
    /// </summary>
    public class FileCache : ICache
    {
        #region Constants
        private const string CACHE_DIRECTORY_NAME = @"cache";
        #endregion

        #region Fields

        /**
	     * The directory for storing cache data.
	     */
        private string _directory;

        #endregion

        #region Factory

        /// <summary>
        /// Create a new <see cref="FileCache"/> with a default directory.
        /// The directory will be the value of the jotify.cache system
        /// property or './cache' if that property is
        /// undefined.
        /// </summary>
        public FileCache()
            : this(Path.Combine(AppDomain.CurrentDomain.BaseDirectory/*Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)*/, CACHE_DIRECTORY_NAME))
        {

        }

        public FileCache(string directory)
        {
            this._directory = directory;
        }

        #endregion

        #region methods

        private void ClearDirectory(string directory, bool includeSubDirs)
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(directory);
                foreach (FileInfo fi in dirInfo.GetFiles())
                {
                    try
                    {
                        fi.Delete();
                    }
                    catch (Exception)
                    {
                        
                    }
                }
                if (includeSubDirs)
                {
                    foreach (DirectoryInfo di in dirInfo.GetDirectories())
                    {
                        try
                        {
                            di.Delete(true);
                        }
                        catch (Exception)
                        {
                            
                        }
                    }
                }
            }
            catch (Exception)
            {
               
            }
        }

        private string GetFullPath(string category, string hash)
        {
            return Path.Combine(Path.Combine(_directory, category), hash);
        }

        #endregion


        #region Cache members

        public void Clear()
        {
            ClearDirectory(_directory, true);
        }

        public void Clear(string category)
        {
            ClearDirectory(Path.Combine(_directory, category), false);
        }

        public bool Contains(string category, string hash)
        {
            return File.Exists(GetFullPath(category, hash));
        }

        public byte[] Load(string category, string hash)
        {
            try
            {
                using (FileStream fs = new FileStream(GetFullPath(category, hash), FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    return buffer;
                }
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
                File.Delete(GetFullPath(category, hash));
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

        private void EnsureDirectories(string fileName)
        {
            DirectoryInfo dirInfo = new FileInfo(fileName).Directory;
            EnsureDirectories(dirInfo);            
        }

        private void EnsureDirectories(DirectoryInfo dir)
        {
            if (!dir.Parent.Exists)
            {
                EnsureDirectories(dir.Parent);
                dir.Create();
            }
            else if (!dir.Exists)
                dir.Create();
        }

        public void Store(string category, string hash, byte[] data, int size)
        {
            try
            {
                string fileName = GetFullPath(category, hash);
                EnsureDirectories(fileName);
                using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(data, 0, size);
                }
            }
            catch (Exception)
            {
                //Ignore errors
            }
        }

        public string[] List(string category)
        {
            List<string> fileList = new List<string>();
            try
            {
                DirectoryInfo di = new DirectoryInfo(Path.Combine(_directory, category));
                foreach (FileInfo fi in di.GetFiles())
                    fileList.Add(fi.Name);
            }
            catch (Exception)
            {
                throw; //FIXME
            }
            return fileList.ToArray();
        }

        #endregion


    }
}
