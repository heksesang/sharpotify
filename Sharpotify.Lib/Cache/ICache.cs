using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sharpotify.Cache
{
    public interface ICache
    {
        /// <summary>
        /// Clear the entire cache.
        /// </summary>
        void Clear();

        /// <summary>
        /// Clear the cache for the specified category.
        /// </summary>
        /// <param name="category">A cache category to clear.</param>
        void Clear(string category);

        /// <summary>
        /// Check if the cache contains an item. 
        /// </summary>
        /// <param name="category">The cache category to check.</param>
        /// <param name="hash">The hash of the item to check.</param>
        /// <returns><code>true</code> if it contains that item, <code>false</code> otherwise.</returns>
        bool Contains(string category, string hash);

        /// <summary>
        /// Load data from the cache.
        /// </summary>
        /// <param name="category">The cache category to load from.</param>
        /// <param name="hash">The hash of the item to load.</param>
        /// <returns>Cached data or <code>null</code>.</returns>
        byte[] Load(string category, string hash);

        /// <summary>
        /// Remove a single item from the cache.
        /// </summary>
        /// <param name="category">The cache category to remove from.</param>
        /// <param name="hash">The hash of the item to remove.</param>
        void Remove(string category, string hash);

        /// <summary>
        /// Store data in the cache.
        /// </summary>
        /// <param name="category">The cache category to store to.</param>
        /// <param name="hash">The hash of the item to store.</param>
        /// <param name="data">The data to store.</param>
        void Store(string category, string hash, byte[] data);

        /// <summary>
        /// Store data in the cache.
        /// </summary>
        /// <param name="category">The cache category to store to.</param>
        /// <param name="hash">The hash of the item to store.</param>
        /// <param name="data">The data to store.</param>
        /// <param name="size">The size of the data.</param>
        void Store(string category, string hash, byte[] data, int size);

        /// <summary>
        /// List data in a cache category.
        /// </summary>
        /// <param name="category">The cache category to list.</param>
        /// <returns>A List of cache hashes.</returns>
        string[] List(string category);
    }
}
