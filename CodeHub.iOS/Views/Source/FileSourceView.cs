using System;
using Xamarin.Utilities.ViewControllers;
using ReactiveUI;
using System.Reactive.Linq;
using Xamarin.Utilities.Views;
using System.Linq;
using MonoTouch.UIKit;
using CodeHub.Core.ViewModels.Source;
using CodeFramework.SourceBrowser;

namespace CodeHub.iOS.Views.Source
{
    public abstract class FileSourceView<TViewModel> : WebView<TViewModel> where TViewModel : class, IFileSourceViewModel
    {
        private bool _fullScreen;
        private UITapGestureRecognizer _tapGesture;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _tapGesture = new UITapGestureRecognizer();
            _tapGesture.ShouldReceiveTouch = (r, t) => t.TapCount == 1;
            _tapGesture.ShouldRecognizeSimultaneously = (a, b) => true;
            _tapGesture.ShouldRequireFailureOf = (a, b) =>
            {
                var targetTap = b as UITapGestureRecognizer;
                if (targetTap != null)
                {
                    return targetTap.NumberOfTapsRequired == 2;
                }
                return false;
            };
            Web.AddGestureRecognizer(_tapGesture);
            _tapGesture.AddTarget(() =>
            {
                _fullScreen = !_fullScreen;
                UIApplication.SharedApplication.SetStatusBarHidden(_fullScreen, UIStatusBarAnimation.Slide);
                NavigationController.SetNavigationBarHidden(_fullScreen, true);
                NavigationController.SetToolbarHidden(_fullScreen, true);
            });

            ViewModel.WhenAnyValue(x => x.Theme)
                .Select(x => new { Theme = x, ViewModel.SourceItem })
                .Skip(1)
                .Where(x => x != null && x.SourceItem != null && !x.SourceItem.IsBinary)
                .Subscribe(x => LoadContent(x.SourceItem.FilePath));

            ViewModel.WhenAnyValue(x => x.SourceItem)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    if (x.IsBinary)
                    {
                        LoadFile(x.FilePath);
                    }
                    else
                    {
                        LoadContent(x.FilePath);
                    }
                });

            ViewModel.WhenAnyObservable(x => x.NextItemCommand.CanExecuteObservable, x => x.PreviousItemCommand.CanExecuteObservable)
                .Where(x => x)
                .Subscribe(_ =>
                {
                    var previousButton = new UIBarButtonItem(Xamarin.Utilities.Images.Images.BackChevron, 
                        UIBarButtonItemStyle.Plain, (s, e) => ViewModel.PreviousItemCommand.ExecuteIfCan());
                    previousButton.EnableIfExecutable(ViewModel.PreviousItemCommand.CanExecuteObservable);

                    var nextButton = new UIBarButtonItem(Xamarin.Utilities.Images.Images.ForwardChevron, 
                        UIBarButtonItemStyle.Plain, (s, e) => ViewModel.NextItemCommand.ExecuteIfCan());
                    nextButton.EnableIfExecutable(ViewModel.NextItemCommand.CanExecuteObservable);

                    ToolbarItems = new []
                    {
                        new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                        previousButton,
                        new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = 80f },
                        nextButton,
                        new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    };

                    NavigationController.SetToolbarHidden(false, true);
                });
        }

        private new void LoadContent(string filePath)
        {
            var content = System.IO.File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            var razorView = new SyntaxHighlighterView 
            { 
                Model = new SourceBrowserModel
                {
                    Content = content,
                    Theme = ViewModel.Theme ?? "idea"
                }
            };

            var html = razorView.GenerateString();
            base.LoadContent(html);
        }

        protected void ShowThemePicker()
        {
            var themes = System.IO.Directory.GetFiles("SourceBrowser/styles")
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

