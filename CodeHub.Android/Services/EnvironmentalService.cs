using CodeHub.Core.Services;

namespace CodeHub.Android.Services
{
    class EnvironmentalService : IEnvironmentalService
    {
        public string ApplicationVersion
        {
            get
            {
                var info = Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0);
                return info.VersionName;
            }
        }
    }
}