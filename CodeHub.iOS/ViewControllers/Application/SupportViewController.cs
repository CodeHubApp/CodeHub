using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.App;
using Humanizer;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.DialogElements;
using System.Reactive;
using CodeHub.iOS.ViewControllers.Repositories;
using MessageUI;
using CodeHub.Core.Services;
using Splat;

namespace CodeHub.iOS.ViewControllers.Application
{
    public class SupportViewController : BaseDialogViewController
    {
        private readonly IAlertDialogService _alertDialogService = Locator.Current.GetService<IAlertDialogService>();

        public SupportViewModel ViewModel { get; } = new SupportViewModel();

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var split = new SplitButtonElement();
            var contributors = split.AddButton("Contributors", "-");
            var lastCommit = split.AddButton("Last Commit", "-");

            var openIssue = new BigButtonElement("Open an Issue on GitHub", Octicon.Bug);
            var sendEmail = new BigButtonElement("Email Support", Octicon.Mail);
            var openIssues = new BigButtonElement("Existing Issues", Octicon.IssueOpened);

            HeaderView.SubText = "This app is the product of hard work and great suggestions! Thank you to all whom provide feedback!";
            HeaderView.Image = UIImage.FromBundle("AppIcons60x60");

            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = "" };

            Root.Reset(
                new Section { split },
                new Section(null, "Opening an issue on the GitHub project page is the fastest way to get a response.") { openIssue, openIssues },
                new Section(),
                new Section(null, "Emails are answered as quickly as possible but there is only one person answering them so expect a delay.") { sendEmail });

            OnActivation(d =>
            {
                d(openIssue.Clicked
                  .Select(_ => new FeedbackComposerViewController())
                  .Select(viewCtrl => new ThemedNavigationController(viewCtrl))
                  .Subscribe(viewCtrl => PresentViewController(viewCtrl, true, null)));

                d(sendEmail.Clicked.Subscribe(_ => SendEmail()));

                d(this.WhenAnyValue(x => x.ViewModel.Title)
                  .Subscribe(title => Title = title));

                d(openIssues.Clicked
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

        private void SendEmail()
        {
            if (!MFMailComposeViewController.CanSendMail)
            {
                _alertDialogService.Alert(
                    "No Email Setup",
                    "Looks like you don't have email setup on this device. " +
                    "Add a mail provider and try again.").ToBackground();
            }
            else
            {
                var ctrl = new MFMailComposeViewController();
                ctrl.SetSubject("CodeHub Support");
                ctrl.SetToRecipients(new[] { "codehubapp@gmail.com" });
                ctrl.Finished += (sender, e) => DismissViewController(true, () =>
                {
                    if (e.Result == MFMailComposeResult.Sent)
                        _alertDialogService.Alert("Sent!", "Thanks for your feedback!");
                });
                PresentViewController(ctrl, true, null);
            }
        }


        private void GoToRepository()
            => this.PushViewController(RepositoryViewController.CreateCodeHubViewController());

        private class BigButtonElement : ButtonElement, IElementSizing
        {
            public BigButtonElement(string name, Octicon img) : base(name, img.ToImage()) { }
            public nfloat GetHeight(UITableView tableView, Foundation.NSIndexPath indexPath) => 60f;
        }
    }
}

