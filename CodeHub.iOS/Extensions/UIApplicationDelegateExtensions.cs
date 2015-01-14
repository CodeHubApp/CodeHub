using System;
using MonoTouch.UIKit;
using MonoTouch.Security;
using MonoTouch.Foundation;

namespace MonoTouch.UIKit
{
    public static class UIApplicationDelegateExtensions
    {
        /// <summary>
        /// Record the date this application was installed (or the date that we started recording installation date).
        /// </summary>
        public static DateTime StampInstallDate(this UIApplicationDelegate @this, string name, string key)
        {
            try
            {
                var query = new SecRecord(SecKind.GenericPassword) { Service = name, Account = key };

                SecStatusCode secStatusCode;
                var queriedRecord = SecKeyChain.QueryAsRecord(query, out secStatusCode);
                if (secStatusCode != SecStatusCode.Success)
                {
                    queriedRecord = new SecRecord(SecKind.GenericPassword)
                    {
                        Label = name + " Install Date",
                        Service = name,
                        Account = key,
                        Description = string.Format("The first date {0} was installed", name),
                        Generic = NSData.FromString(DateTime.UtcNow.ToString())
                    };

                    var err = SecKeyChain.Add(queriedRecord);
                    if (err != SecStatusCode.Success)
                        System.Diagnostics.Debug.WriteLine("Unable to save stamp date!");
                }
                else
                {
                    DateTime time;
                    if (!DateTime.TryParse(queriedRecord.Generic.ToString(), out time))
                        SecKeyChain.Remove(query);
                }

                return DateTime.Parse(NSString.FromData(queriedRecord.Generic, NSStringEncoding.UTF8));
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return DateTime.Now;
            }
        }
    }
}

