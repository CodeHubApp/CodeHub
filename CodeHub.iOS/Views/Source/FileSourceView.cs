using System;
using ReactiveUI;
using System.Reactive.Linq;
using MonoTouch.UIKit;
using CodeHub.Core.ViewModels.Source;
using CodeHub.iOS.WebViews;
using Xamarin.Utilities.Core.Services;

namespace CodeHub.iOS.Views.Source
{
    public abstract class FileSourceView<TViewModel> : ReactiveWebViewController<TViewModel> where TViewModel : FileSourceViewModel
    {
        private bool _fullScreen;

        protected FileSourceView(INetworkActivityService networkActivityService)
            : base(networkActivityService)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

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
    }
}

