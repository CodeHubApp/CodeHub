using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeHub.Core.ViewModels;
using SQLite;

namespace CodeHub.Core.Data
{
    public class AccountFilters : IEnumerable<Filter>
    {
        private readonly SQLiteConnection _sqLiteConnection;

        public AccountFilters(SQLiteConnection sqLiteConnection)
        {
            _sqLiteConnection = sqLiteConnection;
            _sqLiteConnection.CreateTable<Filter>();
        }

        /// <summary>
        /// Gets the filter.
        /// </summary>
        /// <returns>The filter.</returns>
        /// <param name="key">Key.</param>
        public TFilter GetFilter<TFilter>(string key) where TFilter : FilterModel<TFilter>, new()
        {
            Filter filter;
            lock (_sqLiteConnection)
            {
                filter = _sqLiteConnection.Find<Filter>(x => x.Type == key);
            }

            if (filter == null)
                return new TFilter();
            var filterModel = filter.GetData<TFilter>();
            return filterModel ?? new TFilter();
        }

        /// <summary>
        /// Gets the filter.
        /// </summary>
        /// <returns>The filter.</returns>
        /// <param name="key">Key.</param>
        public TFilter GetFilter<TFilter>(object key) where TFilter : FilterModel<TFilter>, new()
        {
            return GetFilter<TFilter>(key.GetType().Name);
        }

        /// <summary>
        /// Gets a filter by it's primary id
        /// </summary>
        /// <returns>The filter.</returns>
        /// <param name="key">Identifier.</param>
        public Filter GetFilter(string key)
        {
            lock (_sqLiteConnection)
            {
                return _sqLiteConnection.Find<Filter>(x => x.Type == key);
            }
        }

        /// <summary>
        /// Adds the filter
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="data">Data.</param>
        public void AddFilter(string key, object data)
        {
            RemoveFilters(key);
            var filter = new Filter { Type = key };
            filter.SetData(data);

            lock (_sqLiteConnection)
            {
                _sqLiteConnection.Insert(filter);
            }
        }

        /// <summary>
        /// Removes the filter
        /// </summary>
        /// <param name="id">Identifier.</param>
        public void RemoveFilter(int id)
        {
            lock (_sqLiteConnection)
            {
                _sqLiteConnection.Delete(new Filter { Id = id });
            }
        }

        /// <summary>
        /// Updates the filter
        /// </summary>
        /// <param name="filter">Filter.</param>
        public void UpdateFilter(Filter filter)
        {
            lock (_sqLiteConnection)
            {
                _sqLiteConnection.Update(filter);
            }
        }

        /// <summary>
        /// Removes all filters with a specific key
        /// </summary>
        /// <param name="key">Key.</param>
        public void RemoveFilters(string key)
        {
            lock (_sqLiteConnection)
            {
                var filters = _sqLiteConnection.Table<Filter>().Where(x => x.Type == key).ToList();
                foreach (var filter in filters)
                    _sqLiteConnection.Delete(filter);
            }
        }

        public IEnumerator<Filter> GetEnumerator()
        {
            lock (_sqLiteConnection)
            {
                return _sqLiteConnection.Table<Filter>().ToList().GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
