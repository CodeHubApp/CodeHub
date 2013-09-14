using SQLite;
using System.Collections.Generic;
using GitHubSharp.Models;
using CodeFramework.Filters.Models;
using System.Linq;

namespace CodeHub.Data
{
    public class Account : CodeFramework.Data.IAccount
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the avatar URL.
        /// </summary>
        /// <value>The avatar URL.</value>
        public string AvatarUrl { get; set; }

		/// <summary>
		/// Gets or sets the password.
		/// </summary>
		/// <value>The password.</value>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the domain
        /// </summary>
        /// <value>The domain.</value>
        public string Domain { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Account"/> dont remember.
		/// THIS HAS TO BE A NEGATIVE STATEMENT SINCE IT DEFAULTS TO 'FALSE' WHEN RETRIEVING A NULL VIA SQLITE
		/// </summary>
		public bool DontRemember { get; set; }

        /// <summary>
        /// Gets or sets whether teams should be listed in the menu controller under 'events'
        /// </summary>
        public bool DontShowTeamEvents { get; set; }

        /// <summary>
        /// Gets or sets whether teams & groups should be expanded in the menu controller to their actual contents
        /// </summary>
        public bool DontExpandTeamsAndGroups { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CodeBucket.Data.Account"/> hides the repository
        /// description in list.
        /// </summary>
        /// <value><c>true</c> if hide repository description in list; otherwise, <c>false</c>.</value>
        public bool HideRepositoryDescriptionInList { get; set; }

        /// <summary>
        /// A transient record of the user's name
        /// </summary>
        [Ignore]
        public string FullName { get; set; }

        /// <summary>
        /// A transient record of the user's organizations
        /// </summary>
        [Ignore]
        public List<BasicUserModel> Organizations { get; set; }

        /// <summary>
        /// A list of the current notifications
        /// </summary>
        /// <value>The notifications.</value>
        [Ignore]
        public int Notifications { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Account"/> class.
		/// </summary>
		public Account()
		{
			//Set some default values
			DontRemember = false;
            DontShowTeamEvents = false;
            DontExpandTeamsAndGroups = false;
            Organizations = new List<BasicUserModel>();
            Notifications = 0;
		}

        
        /// <summary>
        /// Gets the pinned resources.
        /// </summary>
        /// <returns>The pinned resources.</returns>
        /// <param name="c">C.</param>
        public List<PinnedRepository> GetPinnedRepositories()
        {
            return Database.Main.Table<PinnedRepository>().Where(x => x.AccountId == this.Id).OrderBy(x => x.Name).ToList();
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
            var resource = new PinnedRepository { Owner = owner, Slug = slug, Name = name, ImageUri = imageUri, AccountId = this.Id };
            Database.Main.Insert(resource);
        }

        /// <summary>
        /// Removes the pinned repository.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public void RemovePinnedRepository(int id)
        {
            Database.Main.Delete(new PinnedRepository { Id = id });
        }

        /// <summary>
        /// Gets the pinned repository.
        /// </summary>
        /// <returns>The pinned repository.</returns>
        /// <param name="owner">Owner.</param>
        /// <param name="slug">Slug.</param>
        public PinnedRepository GetPinnedRepository(string owner, string slug)
        {
            return Database.Main.Find<PinnedRepository>(x => x.Owner == owner && x.Slug == slug);
        }

        /// <summary>
        /// Gets the filters.
        /// </summary>
        /// <returns>The filters.</returns>
        public List<Filter> GetFilters()
        {
            return Database.Main.Table<Filter>().Where(x => x.AccountId == this.Id).ToList();
        }

        /// <summary>
        /// Gets the filter.
        /// </summary>
        /// <returns>The filter.</returns>
        /// <param name="key">Key.</param>
        public F GetFilter<F>(string key) where F : FilterModel<F>, new()
        {
            var filter = Database.Main.Find<Filter>(x => x.AccountId == this.Id && x.Type == key);
            if (filter == null)
                return new F();
            var filterModel = filter.GetData<F>();
            return filterModel == null ? new F() : filterModel;
        }

        /// <summary>
        /// Gets the filter.
        /// </summary>
        /// <returns>The filter.</returns>
        /// <param name="key">Key.</param>
        public F GetFilter<F>(object key) where F : FilterModel<F>, new()
        {
            return GetFilter<F>(key.GetType().Name);
        }

        /// <summary>
        /// Returns a filter object if it exists, null if otherwise
        /// </summary>
        /// <returns>The exist.</returns>
        /// <param name="key">Key.</param>
        private Filter DoesFilterExist(string key)
        {
            return Database.Main.Find<Filter>(x => x.AccountId == this.Id && x.Type == key);
        }

        /// <summary>
        /// Adds the filter
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="data">Data.</param>
        public void AddFilter<F>(string key, F data)
        {
            RemoveFilters(key);
            var filter = new Filter { AccountId = this.Id, Type = key };
            filter.SetData(data);
            Database.Main.Insert(filter);
        }

        /// <summary>
        /// Adds a filter using any object's type as a key
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="data">Data.</param>
        public void AddFilter<F>(object key, F data)
        {
            AddFilter<F>(key.GetType().Name, data);
        }

        /// <summary>
        /// Removes the filter
        /// </summary>
        /// <param name="id">Identifier.</param>
        public void RemoveFilter(int id)
        {
            Database.Main.Delete(new Filter { Id = id });
        }

        /// <summary>
        /// Removes all filters with a specific key
        /// </summary>
        /// <param name="key">Key.</param>
        public void RemoveFilters(string key)
        {
            var filters = Database.Main.Table<Filter>().Where(x => x.AccountId == this.Id && x.Type == key).ToList();
            foreach (var filter in filters)
                Database.Main.Delete(filter);
        }


        /// <summary>
        /// Returns a <see cref="System.String"/> that represents the current <see cref="Account"/>.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents the current <see cref="Account"/>.</returns>
        public override string ToString()
        {
            return Username;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Account"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Account"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="Account"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            var act = obj as Account;
            return act != null && this.Id.Equals(act.Id);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="Gistacular.Data.Account"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return this.Id;
        }
    }
}

