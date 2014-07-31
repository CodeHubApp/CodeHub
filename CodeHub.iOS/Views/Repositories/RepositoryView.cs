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

            Title = HeaderView.Text = ViewModel.RepositoryName;

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowMenu());
            NavigationItem.RightBarButtonItem.EnableIfExecutable(
                ViewModel.WhenAnyValue(x => x.Repository, x => x.IsStarred, x => x.IsWatched)
                .Select(x => x.Item1 != null && x.Item2 != null && x.Item3 != null));

            _split = new SplitButtonElement();
            var stargazers = _split.AddButton("Stargazers", "-", () => ViewModel.GoToStargazersCommand.ExecuteIfCan());
            var watchers = _split.AddButton("Watchers", "-", () => ViewModel.GoToWatchersCommand.ExecuteIfCan());
            var collaborators = _split.AddButton("Contributors", "-", () => ViewModel.GoToCollaboratorsCommand.ExecuteIfCan());
            ViewModel.WhenAnyValue(x => x.Collaborators).Subscribe(x => collaborators.Text = x.HasValue ? x.ToString() : "-");

            Root.Reset(new Section(HeaderView) { _split });

            ViewModel.WhenAnyValue(x => x.Readme).Where(x => x != null).Subscribe(_ =>
            {
                // Not very efficient but it'll work for now.
                if (ViewModel.Repository != null)
                    Render();
            });

            _splitElements[0] = new SplitElement();
            _splitElements[1] = new SplitElement();
            _splitElements[2] = new SplitElement();
            _splitElements[2].Button2 = new SplitElement.SplitButton(Images.Size, "N/A");

            ViewModel.WhenAnyValue(x => x.Repository).Where(x => x != null).Subscribe(x =>
            {
                HeaderView.ImageUri = x.Owner.AvatarUrl;
                HeaderView.SubText = x.Description;
                stargazers.Text = x.StargazersCount.ToString();
                watchers.Text = x.SubscribersCount.ToString();

                _splitElements[0].Button1 = new SplitElement.SplitButton(x.Private ? Images.Locked : Images.Unlocked, x.Private ? "Private" : "Public");
                _splitElements[0].Button2 = new SplitElement.SplitButton(Images.Language, x.Language ?? "N/A");

                _splitElements[1].Button1 = new SplitElement.SplitButton(Images.Flag, x.OpenIssues + (x.OpenIssues == 1 ? " Issue" : " Issues"));
                _splitElements[1].Button2 = new SplitElement.SplitButton(Images.Fork, x.Forks + (x.Forks == 1 ? " Fork" : " Forks"));

                _splitElements[2].Button1 = new SplitElement.SplitButton(Images.Create, (x.CreatedAt.ToLocalTime()).ToString("MM/dd/yy"));

                Render();
            });

            ViewModel.WhenAnyValue(x => x.RepositorySize).Subscribe(x => _splitElements[2].Button2.Text = x ?? "N/A");

        }

        private void Render()
        {
            var model = ViewModel.Repository;
            var sec1 = new Section();
            sec1.Add(_splitElements);

            var owner = new StyledStringElement("Owner", model.Owner.Login) { Image = Images.Person,  Accessory = UITableViewCellAccessory.DisclosureIndicator };
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
                var web = new StyledStringElement("Website", () => ViewModel.GoToUrlCommand.Execute(model.Homepage), Images.Webpage);
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