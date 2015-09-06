using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeFramework.Core.Cache;
using SQLite;

namespace CodeFramework.Core.Data
{
    public class AccountCache : IEnumerable<CacheEntry>
    {
        private readonly object _lock = new object();
        private readonly SQLiteConnection _sqLiteConnection;
        private readonly string _cachePath;

        public AccountCache(SQLiteConnection sqLiteConnection, string cachePath)
        {
            _sqLiteConnection = sqLiteConnection;
            _cachePath = cachePath;
            _sqLiteConnection.CreateTable<CacheEntry>();

            //Make sure the cachePath exists
            if (!System.IO.Directory.Exists(_cachePath))
                System.IO.Directory.CreateDirectory(_cachePath);
        }

        public CacheEntry GetEntry(string query)
        {
            lock (_lock)
            {
                var queries = _sqLiteConnection.Query<CacheEntry>("select * from CacheEntry where query = ?", query);
                var cacheEntry = queries.FirstOrDefault();

                if (cacheEntry == null)
                    return null;

                if (!cacheEntry.IsValid)
                {
                    //Remove it since it's not valid anymore
                    _sqLiteConnection.Delete(cacheEntry);
                    return null;
                }

                return cacheEntry;
            }
        }

        public byte[] Get(string query)
        {
            var cacheEntry = GetEntry(query);

            try
            {
                return cacheEntry.LoadResult();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Set(string query, byte[] content, string cacheTag = null)
        {
            lock (_lock)
            {
                var queries = _sqLiteConnection.Query<CacheEntry>("select * from CacheEntry where query = ?", query);
                var cacheEntry = queries.FirstOrDefault();
                if (cacheEntry != null)
                {
                    cacheEntry.SaveResult(content);
                    cacheEntry.CacheTag = cacheTag;
                    _sqLiteConnection.Update(cacheEntry);
                }
                else
                {
                    cacheEntry = new CacheEntry { Query = query, Path = System.IO.Path.Combine(_cachePath, System.IO.Path.GetTempFileName()), CacheTag = cacheTag };
                    cacheEntry.SaveResult(content);
                    try
                    {
                        if (_sqLiteConnection.Insert(cacheEntry) != 1)
                            throw new InvalidOperationException("Did not insert cache object");
                    }
                    catch
                    {
                        //Clean up the file
                        cacheEntry.Delete();
                    }
                }
            }
        }

        public void DeleteAll()
        {
            lock (_lock)
            {
                foreach (var entry in _sqLiteConnection.Table<CacheEntry>())
                    entry.Delete();
                _sqLiteConnection.DeleteAll<CacheEntry>();
            }
        }

        public void Delete(string query)
        {
            lock (_lock)
            {
                var cacheEntry = _sqLiteConnection.Table<CacheEntry>().FirstOrDefault(x => x.Query.Equals(query));
                if (cacheEntry != null)
                {
                    cacheEntry.Delete();
                    _sqLiteConnection.Delete(cacheEntry);
                }
            }
        }

        public void DeleteWhereStartingWith(string query)
        {
            lock (_lock)
            {
                foreach (var cacheEntry in _sqLiteConnection.Table<CacheEntry>().Where(x => x.Query.StartsWith(query)).ToList())
                {
                    cacheEntry.Delete();
                    _sqLiteConnection.Delete(cacheEntry);
                }
            }
        }

        public IEnumerator<CacheEntry> GetEnumerator()
        {
            return _sqLiteConnection.Table<CacheEntry>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
