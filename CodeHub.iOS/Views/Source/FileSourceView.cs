using System;
using Xamarin.Utilities.ViewControllers;
using ReactiveUI;
using System.Reactive.Linq;
using Xamarin.Utilities.Views;
using System.Linq;
using MonoTouch.UIKit;
using CodeHub.Core.ViewModels.Source;
using CodeHub.iOS.WebViews;
using MonoTouch.Foundation;

namespace CodeHub.iOS.Views.Source
{
    public abstract class FileSourceView<TViewModel> : WebView<TViewModel> where TViewModel : FileSourceViewModel
    {
        private bool _fullScreen;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ViewModel.WhenAnyValue(x => x.Title).Subscribe(x => Title = x ?? string.Empty);

            ViewModel.WhenAnyValue(x => x.Theme)
                .Select(_ => ViewModel.SourceItem)
                .Skip(1)
                .Where(x => x != null && !x.IsBinary)
                .Subscribe(x => LoadSource(x.FileUri));

            ViewModel.WhenAnyValue(x => x.SourceItem)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    if (x.IsBinary)
                    {
                        LoadFile(x.FileUri.AbsoluteUri);
                    }
                    else
                    {
                        LoadSource(x.FileUri);
                    }
                });
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (_fullScreen && NavigationController != null)
                SetFullScreen(false);
        }

        private void SetFullScreen(bool fullScreen)
        {
            _fullScreen = fullScreen;
            UIApplication.SharedApplication.SetStatusBarHidden(fullScreen, UIStatusBarAnimation.Slide);
            NavigationController.SetNavigationBarHidden(fullScreen, true);
        }

        protected virtual void LoadSource(Uri fileUri)
        {
            var content = System.IO.File.ReadAllText(fileUri.LocalPath, System.Text.Encoding.UTF8);
            var razorView = new SyntaxHighlighterView 
            { 
                Model = new SourceBrowserModel
                {
                    Content = content,
                    Theme = ViewModel.Theme ?? "idea"
                }
            };

            LoadContent(razorView.GenerateString());
        }

        protected void ShowThemePicker()
        {
            var path = System.IO.Path.Combine(NSBundle.MainBundle.BundlePath, "WebResources", "styles");
            if (!System.IO.Directory.Exists(path))
                return;

            var themes = System.IO.Directory.GetFiles(path)
                .Where(x => x.EndsWith(".css", StringComparison.Ordinal))
                .Select(x => System.IO.Path.GetFileNameWithoutExtension(x))
                .ToList();

            var selected = themes.IndexOf(ViewModel.Theme ?? "idea");
            if (selected <= 0)
                selected = 0;

            new PickerAlertView(themes.ToArray(), selected, x =>
            {
                if (x < themes.Count)
                    ViewModel.Theme = themes[x];
            }).Show();
        }
    }
}

