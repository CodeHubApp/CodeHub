using System.Collections.Generic;

namespace CodeHub.Core.Data
{
    public class GitHubAccount
    {
        /// <summary>
        /// Gets or sets the OAuth string
        /// </summary>
        public string OAuth { get; set; }

        /// <summary>
        /// Gets or sets the web domain. Sort of like the API domain with the API paths
        /// </summary>
        /// <value>The web domain.</value>
        public string WebDomain { get; set; }

        /// <summary>
        /// Gets whether this account is enterprise or not
        /// </summary>
        /// <value><c>true</c> if enterprise; otherwise, <c>false</c>.</value>
        public bool IsEnterprise { get; set; }

        /// <summary>
        /// Gets or sets whether orgs should be listed in the menu controller under 'events'
        /// </summary>
        public bool ShowOrganizationsInEvents { get; set; }

        /// <summary>
        /// Gets or sets whether teams & groups should be expanded in the menu controller to their actual contents
        /// </summary>
        public bool ExpandOrganizations { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="GitHubAccount"/> hides the repository
        /// description in list.
        /// </summary>
        /// <value><c>true</c> if hide repository description in list; otherwise, <c>false</c>.</value>
        public bool ShowRepositoryDescriptionInList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CodeHub.Core.Data.GitHubAccount"/> push notifications enabled.
        /// </summary>
        /// <value><c>true</c> if push notifications enabled; otherwise, <c>false</c>.</value>
        public bool? IsPushNotificationsEnabled { get; set; }

        /// <summary>
        /// Get or set the code editing theme
        /// </summary>
        /// <value>The code edit theme.</value>
        public string CodeEditTheme { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubAccount"/> class.
        /// </summary>
        public GitHubAccount()
        {
            //Set some default values
            DontRemember = false;
            ShowOrganizationsInEvents = true;
            ExpandOrganizations = true;
            ShowRepositoryDescriptionInList = true;
        }

        public bool DontRemember { get; set; }

        public string Key
        {
            get { return Domain + Username; }
        }

        public string Username { get; set; }

        public string Password { get; set; }

        public string AvatarUrl { get; set; }

        public string Domain { get; set; }

        public Dictionary<string, object> Filters { get; set; }

        public List<PinnedRepository> PinnnedRepositories { get; set; }

        public string DefaultStartupView { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(GitHubAccount))
                return false;
            var other = (GitHubAccount)obj;
            return Username == other.Username && Domain == other.Domain;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Username != null ? Username.GetHashCode() : 0) ^ (Domain != null ? Domain.GetHashCode() : 0);
            }
        }
        
    }
}

