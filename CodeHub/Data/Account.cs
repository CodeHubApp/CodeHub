using SQLite;
using System.Collections.Generic;
using GitHubSharp.Models;
using CodeFramework.Data;

namespace CodeHub.Data
{
    public class Account : IAccount
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
		/// Initializes a new instance of the <see cref="Account"/> class.
		/// </summary>
		public Account()
		{
			//Set some default values
			DontRemember = false;
            DontShowTeamEvents = false;
            DontExpandTeamsAndGroups = false;
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

