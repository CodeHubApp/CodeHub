using System;
using UIKit;
using Foundation;
using System.IO;
using System.Globalization;

namespace MonoTouch
{
    public static class Utilities
    {
        /// <summary>
        ///   A shortcut to the main application
        /// </summary>
        public static UIApplication MainApp = UIApplication.SharedApplication;

        public readonly static string BaseDir = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "..");

        //
        // Since we are a multithreaded application and we could have many
        // different outgoing network connections (api.twitter, images,
        // searches) we need a centralized API to keep the network visibility
        // indicator state
        //
        static readonly object NetworkLock = new object ();
        static int _active;

        public static void PushNetworkActive ()
        {
            lock (NetworkLock){
                _active++;
                MainApp.NetworkActivityIndicatorVisible = true;
            }
        }

        public static void PopNetworkActive ()
        {
            lock (NetworkLock){
                if (_active == 0)
                    return;

                _active--;
                if (_active == 0)
                    MainApp.NetworkActivityIndicatorVisible = false;
            }
        }

        public static NSUserDefaults Defaults = NSUserDefaults.StandardUserDefaults;

        public static void LogException (string text, Exception e)
        {
            Console.WriteLine (String.Format ("On {0}, message: {1}\nException:\n{2}", DateTime.Now, text, e));
			//Analytics.TrackException(false, e.Message + " - " + e.StackTrace);
        }

        public static void ShowAlert(string title, string message, Action dismissed = null)
        {
            var alert = new UIAlertView {Title = title, Message = message};
            alert.DismissWithClickedButtonIndex(alert.AddButton("Ok".t()), true);
            alert.Dismissed += (sender, e) => {
                dismissed?.Invoke();
                alert.Dispose();
            };
            alert.Show();
        }


        public static bool IsTall
        {
            get 
            { 
                return UIDevice.CurrentDevice.UserInterfaceIdiom 
                    == UIUserInterfaceIdiom.Phone 
                        && UIScreen.MainScreen.Bounds.Height 
                        * UIScreen.MainScreen.Scale >= 1136;
            }     
        }
    }
}

