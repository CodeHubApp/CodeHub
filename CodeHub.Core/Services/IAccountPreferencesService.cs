namespace CodeHub.Core.Services
{
    public interface IAccountPreferencesService
    {
        /// <summary>
        /// Gets the accounts directory
        /// </summary>
        string AccountsDir { get; }
    }
}