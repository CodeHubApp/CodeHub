using System;
using CodeHub.Core.ViewModels.App;
using Xamarin.Utilities.DialogElements;
using System.Reactive.Linq;
using Humanizer;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.iOS.Elements;

namespace CodeHub.iOS.Views.App
{
    public class SupportView : ReactiveDialogViewController<SupportViewModel>
    {
        private readonly SplitButtonElement _split;
        private readonly StyledStringElement _addFeatureButton;
        private readonly StyledStringElement _addBugButton;
        private readonly StyledStringElement _featuresButton;

        public SupportView()
        {
            _split = new SplitButtonElement();
            var contributors = _split.AddButton("Contributors", "-");
            var lastCommit = _split.AddButton("Last Commit", "-");

            _addFeatureButton = new ButtonElement("Suggest a feature", () => ViewModel.GoToSuggestFeatureCommand.ExecuteIfCan(), Images.LightBulb);
            _addBugButton = new ButtonElement("Report a bug", () => ViewModel.GoToReportBugCommand.ExecuteIfCan(), Images.Bug);
            _featuresButton = new ButtonElement("Submitted Work Items", () => ViewModel.GoToFeedbackCommand.ExecuteIfCan(), Images.Clippy);

            this.WhenViewModel(x => x.Contributors).Where(x => x.HasValue).SubscribeSafe(x =>
                contributors.Text = (x.Value >= 100 ? "100+" : x.Value.ToString()));

            this.WhenViewModel(x => x.LastCommit).Where(x => x.HasValue).SubscribeSafe(x =>
                lastCommit.Text = x.Value.UtcDateTime.Humanize());

            this.WhenAnyValue(x => x.ViewModel)
                .IsNotNull().Take(1)
                .Subscribe(x => x.LoadCommand.ExecuteIfCan());

            HeaderView.SubText = "This app is the product of hard work and great suggestions! Thank you to all whom provide feedback!";
            HeaderView.Image = UIImage.FromFile("Icon@2x.png");
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Root.Reset(new Section { _split }, new Section { _addFeatureButton, _addBugButton }, new Section { _featuresButton });
        }

        private class ButtonElement : DialogStringElement, IElementSizing
        {
            public ButtonElement(string name, Action click, UIImage img)
                : base(name, click, img)
            {
            }

            public float GetHeight(UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
            {
                return 64f;
            }
        }
    }
}

