using System;
using Xamarin.Utilities.ViewControllers;
using CodeHub.Core.ViewModels.App;
using Xamarin.Utilities.DialogElements;
using System.Reactive.Linq;
using Humanizer;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Views.App
{
    public class FeedbackView : ViewModelPrettyDialogViewController<FeedbackViewModel>
    {
        private readonly SplitButtonElement _split;
        private readonly StyledStringElement _addFeatureButton;
        private readonly StyledStringElement _addBugButton;
        private readonly StyledStringElement _featuresButton;

        public FeedbackView()
        {
            _split = new SplitButtonElement();
            var contributors = _split.AddButton("Contributors", "-");
            var lastCommit = _split.AddButton("Last Commit", "-");

            _addFeatureButton = new ButtonElement("Suggest a feature", () => {}, Images.Update);
            _addBugButton = new ButtonElement("Report a bug", () => {}, Images.Tag);
            _featuresButton = new ButtonElement("Submitted Work Items", () =>
            {
            }, Images.Chart);

            this.WhenViewModel(x => x.Contributors).Where(x => x.HasValue).SubscribeSafe(x =>
                contributors.Text = (x.Value >= 100 ? "100+" : x.Value.ToString()));

            this.WhenViewModel(x => x.LastCommit).Where(x => x.HasValue).SubscribeSafe(x =>
                lastCommit.Text = x.Value.UtcDateTime.Humanize());
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            HeaderView.SubText = "This app is the product of hard work and great suggestions! Thank you to all whom provide feedback!";
            HeaderView.Image = UIImage.FromFile("Icon@2x.png");

            Root.Reset(new Section(HeaderView) { _split }, new Section { _addFeatureButton, _addBugButton }, new Section { _featuresButton });
        }

        private class ButtonElement : StyledStringElement, IElementSizing
        {
            public ButtonElement(string name, Action click, UIImage img)
                : base(name, click, img)
            {
            }

            public float GetHeight(MonoTouch.UIKit.UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
            {
                return 64f;
            }
        }
    }
}

