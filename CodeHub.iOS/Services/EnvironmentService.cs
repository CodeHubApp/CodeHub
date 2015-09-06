using System;
using CodeFramework.Core.Services;
using Foundation;
using UIKit;

namespace CodeFramework.iOS.Services
{
    public class EnvironmentService : IEnvironmentService
    {
        public string OSVersion
        {
            get
            {
                var v = MonoTouch.Utilities.iOSVersion;
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
                } catch { }

                try
                {
                    bundleVersion = NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();
                } catch { }
       
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

