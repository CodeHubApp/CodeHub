using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Xamarin.Utilities.Services;

namespace CodeHub.iOS.Services
{
    public class EnvironmentalService : IEnvironmentalService
    {
        public static Tuple<int, int> iOSVersion
        {
            get
            {
                try
                {
                    var version = UIDevice.CurrentDevice.SystemVersion.Split('.');
                    var major = Int32.Parse(version[0]);
                    var minor = Int32.Parse(version[1]);
                    return new Tuple<int, int>(major, minor);
                }
                catch (Exception e)
                {
                    return new Tuple<int, int>(5, 0);
                }
            }
        }

        public string OSVersion
        {
            get
            {
                var v = iOSVersion;
                return String.Format("{0}.{1}", v.Item1, v.Item2);
            }
        }

        public string ApplicationVersion
        {
            get
            {
                string shortVersion = string.Empty;
                string bundleVersion = string.Empty;

                try
                {
                    shortVersion = NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString();
                }
                catch { }

                try
                {
                    bundleVersion = NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();
                }
                catch { }

                if (string.Equals(shortVersion, bundleVersion))
                    return shortVersion;

                return string.IsNullOrEmpty(bundleVersion) ? shortVersion : string.Format("{0} ({1})", shortVersion, bundleVersion);
            }
        }

        public string DeviceName
        {
            get
            {
                return UIDevice.CurrentDevice.Name;
            }
        }
    }
}

