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

            var addFeatureButton = new ButtonElement("Suggest a feature", Octicon.LightBulb.ToImage()) { Accessory = UITableViewCellAccessory.DisclosureIndicator };
            var addBugButton = new ButtonElement("Report a bug", Octicon.Bug.ToImage()) { Accessory = UITableViewCellAccessory.DisclosureIndicator };
            var featuresButton = new ButtonElement("Submitted Work Items", Octicon.Clippy.ToImage()) { Accessory = UITableViewCellAccessory.DisclosureIndicator };

            HeaderView.SubText = "This app is the product of hard work and great suggestions! Thank you to all whom provide feedback!";
            HeaderView.Image = UIImage.FromFile("Icon@2x.png");

            Root.Reset(new Section { split }, new Section { addFeatureButton, addBugButton }, new Section { featuresButton });

            OnActivation(d => {
                d(addFeatureButton.Clicked.InvokeCommand(ViewModel.GoToSuggestFeatureCommand));
                d(addBugButton.Clicked.InvokeCommand(ViewModel.GoToReportBugCommand));
                d(featuresButton.Clicked.InvokeCommand(ViewModel.GoToFeedbackCommand));
                d(HeaderView.Clicked.InvokeCommand(ViewModel.GoToRepositoryCommand));

                d(this.WhenAnyValue(x => x.ViewModel.Contributors).Where(x => x.HasValue).SubscribeSafe(x =>
                    contributors.Text = (x.Value >= 100 ? "100+" : x.Value.ToString())));

                d(this.WhenAnyValue(x => x.ViewModel.LastCommit).Where(x => x.HasValue).SubscribeSafe(x =>
                    lastCommit.Text = x.Value.UtcDateTime.Humanize()));
            });
        }

        private class ButtonElement : StringElement, IElementSizing
        {
            public ButtonElement(string name, UIImage img)
                : base(name, img)
            {
            }

            public nfloat GetHeight(UITableView tableView, Foundation.NSIndexPath indexPath)
            {
                return 58f;
            }
        }
    }
}

