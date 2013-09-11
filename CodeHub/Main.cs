using MonoTouch.Foundation;
using MonoTouch.UIKit;
using CodeFramework.Cells;
using CodeFramework.Views;
using CodeFramework.Controllers;
using CodeHub.ViewControllers;

namespace CodeHub
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
        public new UIWindow Window { get; set; }

		public SlideoutNavigationController Slideout { get; set; }

		// This is the main entry point of the application.
		static void Main (string[] args)
		{
			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main (args, null, "AppDelegate");
		}

		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
            //Start the analytics tracker
            MonoTouch.Utilities.SetupAnalytics("", "CodeHub");

			//Set the theme
			SetTheme();

			//Create the window
			Window = new UIWindow(UIScreen.MainScreen.Bounds);

			//Always start into the Startup controller
			Window.RootViewController = new StartupViewController();

			//Make what ever window visible.
			Window.MakeKeyAndVisible();

			//Always return true
			return true;
		}

		
		/// <summary>
		/// Sets the theme of the application.
		/// </summary>
		private void SetTheme()
		{
			//Set the status bar
			UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.BlackOpaque, false);

			//Set the theming
			UINavigationBar.Appearance.SetBackgroundImage(Images.Titlebar.CreateResizableImage(new UIEdgeInsets(0, 0, 1, 0)), UIBarMetrics.Default);

			UIBarButtonItem.Appearance.SetBackgroundImage(Images.BarButton.CreateResizableImage(new UIEdgeInsets(8, 7, 8, 7)), UIControlState.Normal, UIBarMetrics.Default);
			UISegmentedControl.Appearance.SetBackgroundImage(Images.BarButton.CreateResizableImage(new UIEdgeInsets(8, 7, 8, 7)), UIControlState.Normal, UIBarMetrics.Default);

			UIBarButtonItem.Appearance.SetBackgroundImage(Images.BarButtonLandscape.CreateResizableImage(new UIEdgeInsets(8, 7, 8, 7)), UIControlState.Normal, UIBarMetrics.LandscapePhone);
			UISegmentedControl.Appearance.SetBackgroundImage(Images.BarButtonLandscape.CreateResizableImage(new UIEdgeInsets(8, 7, 8, 7)), UIControlState.Normal, UIBarMetrics.LandscapePhone);

			//BackButton
			UIBarButtonItem.Appearance.SetBackButtonBackgroundImage(Images.BackButton.CreateResizableImage(new UIEdgeInsets(0, 14, 0, 5)), UIControlState.Normal, UIBarMetrics.Default);

			UISegmentedControl.Appearance.SetDividerImage(Images.Divider, UIControlState.Normal, UIControlState.Normal, UIBarMetrics.Default);

			UIToolbar.Appearance.SetBackgroundImage(Images.Bottombar.CreateResizableImage(new UIEdgeInsets(0, 0, 0, 0)), UIToolbarPosition.Bottom, UIBarMetrics.Default);
			UISearchBar.Appearance.BackgroundImage = Images.Searchbar;

			SearchFilterBar.ButtonBackground = Images.BarButton.CreateResizableImage(new UIEdgeInsets(0, 6, 0, 6));

            MonoTouch.Dialog.StyledStringElement.BgColor = UIColor.FromPatternImage(Images.TableCell);
            MonoTouch.Dialog.StyledStringElement.DefaultTitleFont = UIFont.SystemFontOfSize(14f); //UIColor.FromPatternImage(Images.TableCell);
            MonoTouch.Dialog.StyledStringElement.DefaultDetailFont = UIFont.SystemFontOfSize(14f); //UIColor.FromPatternImage(Images.TableCell);

            RepositoryCellView.RoundImages = false;

            //Set the theme
            Theme.Setup();

            UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes { 
                Font = UIFont.SystemFontOfSize(20f), 
                TextColor = Theme.CurrentTheme.NavigationTextColor,
                TextShadowColor = UIColor.White, 
                TextShadowOffset = new UIOffset(0, 1) 
            });

            UISegmentedControl.Appearance.SetTitleTextAttributes(new UITextAttributes { 
                Font = UIFont.SystemFontOfSize(14f), 
                TextColor = UIColor.FromRGB(87, 85, 85), 
                TextShadowColor = UIColor.FromRGBA(255, 255, 255, 125), 
                TextShadowOffset = new UIOffset(0, 1) 
            }, UIControlState.Normal);

//            try
//            {
//                UINavigationBar.Appearance.ShadowImage = new UIImage();
//                UIToolbar.Appearance.SetShadowImage(new UIImage(), UIToolbarPosition.Bottom);
//            }
//            catch {
//            }

			//Resize the back button only on the iPhone
			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
			{
				UIBarButtonItem.Appearance.SetBackButtonBackgroundImage(Images.BackButtonLandscape.CreateResizableImage(new UIEdgeInsets(0, 14, 0, 6)), UIControlState.Normal, UIBarMetrics.LandscapePhone);
			}
		}

		public override void ReceiveMemoryWarning(UIApplication application)
		{
			//Remove everything from the cache
            if (Application.Client != null && Application.Client.CacheProvider != null)
                Application.Client.CacheProvider.DeleteAll();

			//Pop back to the root view...
//			if (Slideout != null && Slideout.TopView != null && Slideout.TopView.NavigationController != null)
//				Slideout.TopView.NavigationController.PopToRootViewController(false);
		}
	}
}
