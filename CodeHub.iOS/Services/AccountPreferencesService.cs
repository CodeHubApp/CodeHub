using CodeHub.Core.Services;
using System.IO;
using System;

namespace CodeHub.iOS.Services
{
    public class AccountPreferencesService : IAccountPreferencesService
    {
        public readonly static string BaseDir = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "..");

        public string AccountsDir
        {
            get { return System.IO.Path.Combine(BaseDir, "Documents/accounts"); }
        }
    }
}
