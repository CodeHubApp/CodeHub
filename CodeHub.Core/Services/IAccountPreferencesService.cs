namespace CodeFramework.Core.Services
{
    public interface IAccountPreferencesService
    {
        /// <summary>
        /// Gets the accounts directory
        /// </summary>
        string AccountsDir { get; }

        /// <summary>
        /// Gets the cache directory for the accounts
        /// </summary>
        string CacheDir { get; } 
    }
}