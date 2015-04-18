using System;
using SQLite;
using System.IO;
using CodeHub.Core.Data;
using System.Linq;

namespace CodeHub.iOS.Data
{
    public static class LegacyMigration
    {
        public static void Migrate(IAccountsRepository accounts)
        {
            var accountsDir = AccountPreferencesService.AccountsDir;
            if (!Directory.Exists(accountsDir))
                return;

            var accountsDb = Path.Combine(accountsDir, "accounts.db");
            if (!File.Exists(accountsDb))
                return;

            var db = new SQLiteConnection(accountsDb);
            foreach (var account in db.Table<GitHubAccount>())
            {
                // Create a new account
                accounts.Insert(new CodeHub.Core.Data.GitHubAccount
                {
                    AvatarUrl = account.AvatarUrl,
                    Username = account.Username,
                    ShowOrganizationsInEvents = account.ShowOrganizationsInEvents,
                    ShowRepositoryDescriptionInList = account.ShowRepositoryDescriptionInList,
                    IsPushNotificationsEnabled = account.IsPushNotificationsEnabled,
                    ExpandOrganizations = account.ExpandOrganizations,
                    IsEnterprise = account.IsEnterprise,
                    Domain = account.Domain,
                    WebDomain = account.WebDomain,
                    OAuth = account.OAuth,
                    PinnnedRepositories = account.PinnnedRepositories.Select(x =>
                        new CodeHub.Core.Data.PinnedRepository() {
                            Name = x.Name,
                            Owner = x.Owner,
                            Slug = x.Slug
                        }).ToList()
                });
            }

            File.Delete(accountsDb);
        }
    }
}

