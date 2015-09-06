namespace CodeFramework.Core.Data
{
    public interface IAccount
    {
        int Id { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        string Username { get; set; }

        /// <summary>
        /// Gets or sets the avatar URL.
        /// </summary>
        /// <value>The avatar URL.</value>
        string AvatarUrl { get; set; }

        /// <summary>
        /// Gets the filters
        /// </summary>
        AccountFilters Filters { get; }

		/// <summary>
		/// Gets the pinned repositories
		/// </summary>
		/// <value>The pinnned repositories.</value>
		AccountPinnedRepositories PinnnedRepositories { get; }

		/// <summary>
		/// Gets or sets the default startup view.
		/// </summary>
		/// <value>The default startup view.</value>
		string DefaultStartupView { get; set; }
    }
}