using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.App;
using Humanizer;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.DialogElements;
using System.Reactive;
using CodeHub.iOS.ViewControllers.Repositories;

namespace CodeHub.iOS.ViewControllers.Application
{
    public class SupportViewController : BaseDialogViewController
    {
        public SupportViewModel ViewModel { get; } = new SupportViewModel();

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var split = new SplitButtonElement();
            var contributors = split.AddButton("Contributors", "-");
            var lastCommit = split.AddButton("Last Commit", "-");

            var addFeatureButton = new BigButtonElement("Suggest a feature", Octicon.LightBulb);
            var addBugButton = new BigButtonElement("Report a bug", Octicon.Bug);
            var featuresButton = new BigButtonElement("Submitted Work Items", Octicon.Clippy);

            HeaderView.SubText = "This app is the product of hard work and great suggestions! Thank you to all whom provide feedback!";
            HeaderView.Image = UIImage.FromBundle("AppIcons60x60");

            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = "" };

            Root.Reset(
                new Section { split },
                new Section { addFeatureButton, addBugButton },
                new Section { featuresButton });

            OnActivation(d =>
            {
                d(addFeatureButton.Clicked
                  .Select(_ => FeedbackComposerViewController.CreateAsFeature())
                  .Select(viewCtrl => new ThemedNavigationController(viewCtrl))
                  .Subscribe(viewCtrl => PresentViewController(viewCtrl, true, null)));

                d(addBugButton.Clicked
                  .Select(_ => FeedbackComposerViewController.CreateAsBug())
                  .Select(viewCtrl => new ThemedNavigationController(viewCtrl))
                  .Subscribe(viewCtrl => PresentViewController(viewCtrl, true, null)));

                d(this.WhenAnyValue(x => x.ViewModel.Title)
                  .Subscribe(title => Title = title));

                d(featuresButton.Clicked
                  .Subscribe(_ => this.PushViewController(new FeedbackViewController())));

                d(HeaderView.Clicked.Subscribe(_ => GoToRepository()));

                d(this.WhenAnyValue(x => x.ViewModel.Contributors)
                  .Where(x => x.HasValue)
                  .Subscribe(x => contributors.Text = (x.Value >= 100 ? "100+" : x.Value.ToString())));

                d(this.WhenAnyValue(x => x.ViewModel.LastCommit)
                  .Where(x => x.HasValue)
                  .Subscribe(x => lastCommit.Text = x.Value.UtcDateTime.Humanize()));
            });

            Appearing
                .Take(1)
                .Select(_ => Unit.Default)
                .InvokeReactiveCommand(ViewModel.LoadCommand);
        }

        private void GoToRepository()
            => this.PushViewController(RepositoryViewController.CreateCodeHubViewController());

        private class BigButtonElement : ButtonElement, IElementSizing
        {
            public BigButtonElement(string name, Octicon img) : base(name, img.ToImage()) { }
            public nfloat GetHeight(UITableView tableView, Foundation.NSIndexPath indexPath) => 58f;
        }
    }
}

