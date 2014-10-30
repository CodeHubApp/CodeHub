using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp.Models;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoryView : ViewModelPrettyDialogViewController<RepositoryViewModel>
    {
        private UIActionSheet _actionSheet;
        private SplitButtonElement _split;
        private SplitElement[] _splitElements = new SplitElement[3];

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowMenu());
            NavigationItem.RightBarButtonItem.EnableIfExecutable(
                ViewModel.WhenAnyValue(x => x.Repository, x => x.IsStarred, x => x.IsWatched)
                .Select(x => x.Item1 != null && x.Item2 != null && x.Item3 != null));

            _split = new SplitButtonElement();
            var stargazers = _split.AddButton("Stargazers", "-", () => ViewModel.GoToStargazersCommand.ExecuteIfCan());
            var watchers = _split.AddButton("Watchers", "-", () => ViewModel.GoToWatchersCommand.ExecuteIfCan());
            var forks = _split.AddButton("Forks", "-", () => ViewModel.GoToForksCommand.ExecuteIfCan());

            Root.Reset(new Section(HeaderView) { _split });

            // Not very efficient but it'll work for now.
            ViewModel.WhenAnyValue(x => x.Readme).IsNotNull()
                .Select(_ => ViewModel.Repository).IsNotNull().Subscribe(_ => Render());

            _splitElements[0] = new SplitElement();
            _splitElements[0].Button1 = new SplitElement.SplitButton(Images.Locked, string.Empty);
            _splitElements[0].Button2 = new SplitElement.SplitButton(Images.Language, string.Empty);

            _splitElements[1] = new SplitElement();
            _splitElements[1].Button1 = new SplitElement.SplitButton(Images.Flag, string.Empty, () => ViewModel.GoToIssuesCommand.ExecuteIfCan());
            _splitElements[1].Button2 = new SplitElement.SplitButton(Images.Team, string.Empty, () => ViewModel.GoToContributors.ExecuteIfCan());

            _splitElements[2] = new SplitElement();
            _splitElements[2].Button1 = new SplitElement.SplitButton(Images.Tag, string.Empty, () => ViewModel.GoToReleasesCommand.ExecuteIfCan());
            _splitElements[2].Button2 = new SplitElement.SplitButton(Images.Branch, string.Empty, () => ViewModel.GoToBranchesCommand.ExecuteIfCan());

            ViewModel.WhenAnyValue(x => x.Repository).Where(x => x != null).Subscribe(x =>
            {
                HeaderView.ImageUri = x.Owner.AvatarUrl;
                HeaderView.SubText = x.Description;
                stargazers.Text = x.StargazersCount.ToString();
                watchers.Text = x.SubscribersCount.ToString();
                forks.Text = x.ForksCount.ToString();

                _splitElements[0].Button1.Image = x.Private ? Images.Locked : Images.Unlocked;
                _splitElements[0].Button1.Text = x.Private ? "Private" : "Public";
                _splitElements[0].Button2.Text = x.Language ?? "N/A";
                _splitElements[1].Button1.Text = x.OpenIssues + (x.OpenIssues == 1 ? " Issue" : " Issues");

                Render();
            });

            ViewModel.WhenAnyValue(x => x.Contributors).Where(x => x.HasValue).SubscribeSafe(x => 
                _splitElements[1].Button2.Text = (x >= 100 ? "100+" : x.ToString()) + (x == 1 ? " Contributor" : " Contributors"));

            ViewModel.WhenAnyValue(x => x.Branches).Where(x => x != null).SubscribeSafe(x => 
                _splitElements[2].Button2.Text = (x.Count >= 100 ? "100+" : x.Count.ToString()) + (x.Count == 1 ? " Branch" : " Branches"));

            ViewModel.WhenAnyValue(x => x.Releases).Where(x => x.HasValue).SubscribeSafe(x => 
                _splitElements[2].Button1.Text = (x >= 100 ? "100+" : x.ToString()) + (x == 1 ? " Release" : " Releases"));

            ViewModel.WhenAnyValue(x => x.RepositoryName).Subscribe(x => HeaderView.Text = x);
        }

        private void Render()
        {
            var model = ViewModel.Repository;
            var sec1 = new Section();
            sec1.Add(_splitElements);

            var owner = new StyledStringElement("Owner", model.Owner.Login) { Image = Images.User,  Accessory = UITableViewCellAccessory.DisclosureIndicator };
            owner.Tapped += () => ViewModel.GoToOwnerCommand.ExecuteIfCan();
            sec1.Add(owner);

            if (model.Parent != null)
            {
                var parent = new StyledStringElement("Forked From", model.Parent.FullName) { Image = Images.Fork,  Accessory = UITableViewCellAccessory.DisclosureIndicator };
                parent.Tapped += () => ViewModel.GoToForkParentCommand.Execute(model.Parent);
                sec1.Add(parent);
            }

            var events = new StyledStringElement("Events", () => ViewModel.GoToEventsCommand.ExecuteIfCan(), Images.Event);
            var sec2 = new Section { events };

            if (model.HasIssues)
            {
                sec2.Add(new StyledStringElement("Issues", () => ViewModel.GoToIssuesCommand.ExecuteIfCan(), Images.Flag));
            }

            if (ViewModel.Readme != null)
                sec2.Add(new StyledStringElement("Readme", () => ViewModel.GoToReadmeCommand.ExecuteIfCan(), Images.File));

            var sec3 = new Section
            {
                new StyledStringElement("Commits", () => ViewModel.GoToCommitsCommand.ExecuteIfCan(), Images.Commit),
                new StyledStringElement("Pull Requests", () => ViewModel.GoToPullRequestsCommand.ExecuteIfCan(), Images.Hand),
                new StyledStringElement("Source", () => ViewModel.GoToSourceCommand.ExecuteIfCan(), Images.Script),
            };

            Root.Reset(new Section(HeaderView) { _split }, sec1, sec2, sec3);

            if (!string.IsNullOrEmpty(model.Homepage))
            {
                var web = new StyledStringElement("Website", ViewModel.GoToHomepageCommand.ExecuteIfCan, Images.Webpage);
                Root.Add(new Section { web });
            }
        }

        private void ShowMenu()
        {
            _actionSheet = new UIActionSheet(ViewModel.Repository.Name);
            var pinButton = _actionSheet.AddButton(ViewModel.IsPinned ? "Unpin from Slideout Menu" : "Pin to Slideout Menu");
            var starButton = _actionSheet.AddButton(ViewModel.IsStarred.Value ? "Unstar This Repo" : "Star This Repo");
            var watchButton = _actionSheet.AddButton(ViewModel.IsWatched.Value ? "Unwatch This Repo" : "Watch This Repo");
            //var forkButton = sheet.AddButton("Fork Repository");
            var showButton = _actionSheet.AddButton("Show in GitHub");
            var cancelButton = _actionSheet.AddButton("Cancel");
            _actionSheet.CancelButtonIndex = cancelButton;
            _actionSheet.DismissWithClickedButtonIndex(cancelButton, true);
            _actionSheet.Clicked += (s, e) => {
                // Pin to menu
                if (e.ButtonIndex == pinButton)
                    ViewModel.PinCommand.ExecuteIfCan();
                else if (e.ButtonIndex == starButton)
                    ViewModel.ToggleStarCommand.ExecuteIfCan();
                else if (e.ButtonIndex == watchButton)
                    ViewModel.ToggleWatchCommand.ExecuteIfCan();
                else if (e.ButtonIndex == showButton)
                    ViewModel.GoToHtmlUrlCommand.ExecuteIfCan();

                _actionSheet = null;
            };

            _actionSheet.ShowInView(View);
        }
    }
}