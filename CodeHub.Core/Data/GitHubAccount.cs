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
        /// Gets or sets the code hub theme.
        /// </summary>
        public string CodeHubTheme { get; set; }

        /// <summary>
        /// Gets or sets the name of the user
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the email of the user
        /// </summary>
        /// <value>The email.</value>
        public string Email { get; set; }

        /// <summary>
        /// The default view on startup
        /// </summary>
        /// <value>The default startup view.</value>
        public string DefaultStartupView { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CodeHub.Core.Data.GitHubAccount"/> save credentails.
        /// </summary>
        /// <value><c>true</c> if save credentails; otherwise, <c>false</c>.</value>
        public bool SaveCredentails { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubAccount"/> class.
        /// </summary>
        public GitHubAccount()
        {
            //Set some default values
            SaveCredentails = true;
            ShowOrganizationsInEvents = true;
            ExpandOrganizations = true;
            ShowRepositoryDescriptionInList = true;
            DefaultStartupView = "News";
            CodeHubTheme = "Default";
        }

        public string Key
        {
            get { return Domain + Username; }
        }

        public string Username { get; set; }

        public string Password { get; set; }

        public string AvatarUrl { get; set; }

        public string Domain { get; set; }

        private Dictionary<string, object> _filters = new Dictionary<string, object>();
        public Dictionary<string, object> Filters 
        {
            get { return _filters; }
            set { _filters = value ?? new Dictionary<string, object>(); }
        }

        private List<PinnedRepository> _pinnedRepositories = new List<PinnedRepository>();
        public List<PinnedRepository> PinnnedRepositories
        {
            get { return _pinnedRepositories; }
            set { _pinnedRepositories = value ?? new List<PinnedRepository>(); }
        }


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

