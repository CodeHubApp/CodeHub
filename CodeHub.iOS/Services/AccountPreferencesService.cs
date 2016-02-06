using CodeHub.Core.Services;
using MonoTouch;

namespace CodeHub.iOS.Services
{
    public class AccountPreferencesService : IAccountPreferencesService
    {
        public string AccountsDir
        {
            get { return System.IO.Path.Combine(Utilities.BaseDir, "Documents/accounts"); }
        }

        public string CacheDir
        {
            get { return System.IO.Path.Combine(Utilities.BaseDir, "Library/Caches/codeframework.cache/"); }
        }
    }
}
