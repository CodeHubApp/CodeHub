using System;
using CodeHub.Core.ViewModels.Gists;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.Core.Services;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Gists
{
    public class GistViewableFileView : WebView<GistViewableFileViewModel>
    {
        private readonly IAlertDialogService _alertDialogService;

        public GistViewableFileView(IAlertDialogService alertDialogService)
        {
            _alertDialogService = alertDialogService;
        }

        public override void ViewDidLoad()
        {
            NavigationItem.RightBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.ViewButton, UIBarButtonItemStyle.Plain, (s, e) => 
                ViewModel.GoToFileSourceCommand.ExecuteIfCan());

            base.ViewDidLoad();

            ViewModel.WhenAnyValue(x => x.GistFile).Subscribe(x => Title = x.Filename);
            ViewModel.WhenAnyValue(x => x.FilePath).Subscribe(x => LoadFile(x));
        }

        protected override void OnLoadError(object sender, UIWebErrorArgs e)
        {
            base.OnLoadError(sender, e);
            _alertDialogService.Alert("Error", "Unable to display this type of file.");
        }
    }
}