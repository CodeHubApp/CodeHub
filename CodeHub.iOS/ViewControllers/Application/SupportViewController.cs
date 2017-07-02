using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.App;
using Humanizer;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.DialogElements;
using System.Reactive;

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

            var addFeatureButton = new BigButtonElement("Suggest a feature", Octicon.LightBulb.ToImage());
            var addBugButton = new BigButtonElement("Report a bug", Octicon.Bug.ToImage());
            var featuresButton = new BigButtonElement("Submitted Work Items", Octicon.Clippy.ToImage());

            HeaderView.SubText = "This app is the product of hard work and great suggestions! Thank you to all whom provide feedback!";
            HeaderView.Image = UIImage.FromBundle("AppIcons60x60");

            Root.Reset(new Section { split }, new Section { addFeatureButton, addBugButton }, new Section { featuresButton });

            OnActivation(d =>
            {
                //d(addFeatureButton.Clicked.InvokeCommand(ViewModel.GoToSuggestFeatureCommand));
                //d(addBugButton.Clicked.InvokeCommand(ViewModel.GoToReportBugCommand));
                d(featuresButton.Clicked
                  .Subscribe(_ => NavigationController?.PushViewController(new FeedbackViewController(), true)));
                
                //d(HeaderView.Clicked.InvokeCommand(ViewModel.GoToRepositoryCommand));

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
                .InvokeCommand(ViewModel.LoadCommand);
        }

        private class BigButtonElement : ButtonElement, IElementSizing
        {
            public BigButtonElement(string name, UIImage img) : base(name, img) { }
            public nfloat GetHeight(UITableView tableView, Foundation.NSIndexPath indexPath) => 58f;
        }
    }
}

