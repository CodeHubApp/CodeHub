using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SQLite;

namespace CodeHub.Core.Data
{
    public class AccountPinnedRepositories : IEnumerable<PinnedRepository>
    {
        private readonly SQLiteConnection _sqLiteConnection;

        public AccountPinnedRepositories(SQLiteConnection sqLiteConnection)
        {
            _sqLiteConnection = sqLiteConnection;
            _sqLiteConnection.CreateTable<PinnedRepository>();
        }

        /// <summary>
        /// Adds the pinned repository.
        /// </summary>
        /// <param name="owner">Owner.</param>
        /// <param name="slug">Slug.</param>
        /// <param name="name">Name.</param>
        /// <param name="imageUri">Image URI.</param>
        public void AddPinnedRepository(string owner, string slug, string name, string imageUri)
        {
            var resource = new PinnedRepository { Owner = owner, Slug = slug, Name = name, ImageUri = imageUri };
            lock (_sqLiteConnection)
            {
                _sqLiteConnection.Insert(resource);
            }
        }

        /// <summary>
        /// Removes the pinned repository.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public void RemovePinnedRepository(int id)
        {
            lock (_sqLiteConnection)
            {
                _sqLiteConnection.Delete(new PinnedRepository { Id = id });
            }
        }

        /// <summary>
        /// Gets the pinned repository.
        /// </summary>
        /// <returns>The pinned repository.</returns>
        /// <param name="owner">Owner.</param>
        /// <param name="slug">Slug.</param>
        public PinnedRepository GetPinnedRepository(string owner, string slug)
        {
            lock (_sqLiteConnection)
            {
                return _sqLiteConnection.Find<PinnedRepository>(x => x.Owner == owner && x.Slug == slug);
            }
        }

        public IEnumerator<PinnedRepository> GetEnumerator()
        {
            lock (_sqLiteConnection)
            {
                return _sqLiteConnection.Table<PinnedRepository>().OrderBy(x => x.Name).ToList().GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
