using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.App;
using Humanizer;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.ViewControllers.App
{
    public class SupportViewController : BaseDialogViewController<SupportViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var split = new SplitButtonElement();
            var contributors = split.AddButton("Contributors", "-");
            var lastCommit = split.AddButton("Last Commit", "-");

            var addFeatureButton = new ButtonElement("Suggest a feature", () => ViewModel.GoToSuggestFeatureCommand.ExecuteIfCan(), Octicon.LightBulb.ToImage());
            var addBugButton = new ButtonElement("Report a bug", () => ViewModel.GoToReportBugCommand.ExecuteIfCan(), Octicon.Bug.ToImage());
            var featuresButton = new ButtonElement("Submitted Work Items", () => ViewModel.GoToFeedbackCommand.ExecuteIfCan(), Octicon.Clippy.ToImage());

            this.WhenAnyValue(x => x.ViewModel.Contributors).Where(x => x.HasValue).SubscribeSafe(x =>
                contributors.Text = (x.Value >= 100 ? "100+" : x.Value.ToString()));

            this.WhenAnyValue(x => x.ViewModel.LastCommit).Where(x => x.HasValue).SubscribeSafe(x =>
                lastCommit.Text = x.Value.UtcDateTime.Humanize());

            this.WhenAnyValue(x => x.ViewModel).Subscribe(x => 
                HeaderView.ImageButtonAction = x != null ? new Action(x.GoToRepositoryCommand.ExecuteIfCan) : null);

            HeaderView.SubText = "This app is the product of hard work and great suggestions! Thank you to all whom provide feedback!";
            HeaderView.Image = UIImage.FromFile("Icon@2x.png");

            Root.Reset(new Section { split }, new Section { addFeatureButton, addBugButton }, new Section { featuresButton });
        }

        private class ButtonElement : StringElement, IElementSizing
        {
            public ButtonElement(string name, Action click, UIImage img)
                : base(name, click, img)
            {
            }

            public nfloat GetHeight(UITableView tableView, Foundation.NSIndexPath indexPath)
            {
                return 58f;
            }
        }
    }
}

