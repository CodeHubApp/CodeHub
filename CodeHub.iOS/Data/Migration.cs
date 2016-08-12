using System;
using System.IO;
using System.Linq;
using CodeHub.Core.Services;
using MvvmCross.Platform;
using Plugin.Settings;
using SQLite;

namespace CodeHub.iOS.Data
{
    public static class Migration
    {
        public static void Migrate()
        {
            var accounts = Mvx.Resolve<IAccountsService>();
            var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "..");
            var accountsDir = Path.Combine(baseDir, "Documents/accounts");
            var accountsDbPath = Path.Combine(accountsDir, "accounts.db");

            if (!File.Exists(accountsDbPath))
            {
                return;
            }

            try
            {

                var defaultAccountId = CrossSettings.Current.GetValueOrDefault("DEFAULT_ACCOUNT", -1);

                using (var db = new SQLiteConnection(accountsDbPath))
                {
                    foreach (var account in db.Table<GitHubAccount>())
                    {
                        var newAccount = new Core.Data.Account
                        {
                            AvatarUrl = account.AvatarUrl,
                            DefaultStartupView = account.DefaultStartupView,
                            Domain = account.Domain,
                            ExpandOrganizations = account.ExpandOrganizations,
                            IsEnterprise = account.IsEnterprise,
                            IsPushNotificationsEnabled = account.IsPushNotificationsEnabled,
                            OAuth = account.OAuth,
                            Password = account.Password,
                            ShowOrganizationsInEvents = account.ShowOrganizationsInEvents,
                            ShowRepositoryDescriptionInList = account.ShowRepositoryDescriptionInList,
                            Username = account.Username,
                            WebDomain = account.WebDomain,
                            Filters = account.Filters.ToDictionary(x => x.Type, x => new Core.Data.Filter { RawData = x.RawData }),
                            PinnedRepositories = account.PinnnedRepositories.Select(x => new Core.Data.PinnedRepository
                            {
                                ImageUri = x.ImageUri,
                                Name = x.Name,
                                Owner = x.Owner,
                                Slug = x.Slug
                            }).ToList()
                        };

                        accounts.Save(newAccount).ToBackground();

                        if (account.Id == defaultAccountId)
                        {
                            accounts.SetActiveAccount(newAccount).ToBackground();
                        }
                    }

                    db.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error migrating: " + e.Message);
            }
            finally
            {
                File.Delete(accountsDbPath);
                Directory.Delete(accountsDir, true);
            }
        }
    }
}

