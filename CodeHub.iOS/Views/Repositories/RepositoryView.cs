using System;
using System.Reactive.Linq;
using CodeFramework.iOS.ViewComponents;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Repositories;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using ReactiveUI;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoryView : ViewModelDialogView<RepositoryViewModel>
    {
		private readonly HeaderView _header = new HeaderView();
        private UIActionSheet _actionSheet;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu());
            NavigationItem.RightBarButtonItem.EnableIfExecutable(ViewModel.WhenAnyValue(x => x.Repository, x => x != null));

            ViewModel.WhenAnyValue(x => x.Repository).Where(x => x != null).Subscribe(x =>
            {
				ViewModel.ImageUrl = (x.Fork ? Images.GitHubRepoForkUrl : Images.GitHubRepoUrl).AbsoluteUri;
                _header.Subtitle = "Updated " + (ViewModel.Repository.UpdatedAt).ToDaysAgo();
                Render();
            });

            ViewModel.WhenAnyValue(x => x.ImageUrl).Subscribe(x => _header.ImageUri = x);

            ViewModel.WhenAnyValue(x => x.Readme).Where(x => x != null).Subscribe(_ =>
            {
                // Not very efficient but it'll work for now.
                if (ViewModel.Repository != null)
                    Render();
            });
        }

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			Title = _header.Title = ViewModel.RepositoryName;
		}

        private void ShowExtraMenu()
        {
            var repoModel = ViewModel.Repository;
            if (repoModel == null || ViewModel.IsStarred == null || ViewModel.IsWatched == null)
                return;

            var sheet = _actionSheet = new UIActionSheet(repoModel.Name);
			var pinButton = sheet.AddButton(ViewModel.IsPinned ? "Unpin from Slideout Menu" : "Pin to Slideout Menu");
            var starButton = sheet.AddButton(ViewModel.IsStarred.Value ? "Unstar This Repo" : "Star This Repo");
            var watchButton = sheet.AddButton(ViewModel.IsWatched.Value ? "Unwatch This Repo" : "Watch This Repo");
            //var forkButton = sheet.AddButton("Fork Repository");
            var showButton = sheet.AddButton("Show in GitHub");
            var cancelButton = sheet.AddButton("Cancel");
            sheet.CancelButtonIndex = cancelButton;
            sheet.DismissWithClickedButtonIndex(cancelButton, true);
            sheet.Clicked += (s, e) => {
                // Pin to menu
                if (e.ButtonIndex == pinButton)
                {
                    ViewModel.PinCommand.ExecuteIfCan();
                }
                else if (e.ButtonIndex == starButton)
                {
                    ViewModel.ToggleStarCommand.ExecuteIfCan();
                }
                else if (e.ButtonIndex == watchButton)
                {
                    ViewModel.ToggleWatchCommand.ExecuteIfCan();
                }
                // Fork this repo
//                else if (e.ButtonIndex == forkButton)
//                {
//                    ForkRepository();
//                }
                // Show in Bitbucket
                else if (e.ButtonIndex == showButton)
                {
					ViewModel.GoToHtmlUrlCommand.ExecuteIfCan();
                }

                _actionSheet = null;
            };

            sheet.ShowInView(this.View);
        }

        private void Render()
        {
            var model = ViewModel.Repository;
            var root = new RootElement(ViewModel.RepositoryName) { UnevenRows = true };
            root.Add(new Section(_header));
            var sec1 = new Section();

            if (!string.IsNullOrEmpty(model.Description) && !string.IsNullOrWhiteSpace(model.Description))
            {
                var element = new MultilinedElement(model.Description)
                {
                    BackgroundColor = UIColor.White,
                    CaptionColor = Theme.CurrentTheme.MainTitleColor, 
                    ValueColor = Theme.CurrentTheme.MainTextColor
                };
                element.CaptionColor = element.ValueColor;
                element.CaptionFont = element.ValueFont;
                sec1.Add(element);
            }
            //
            //            sec1.Add(new SplitElement(new SplitElement.Row {
            //                Text1 = model.Private ? "Private" : "Public",
            //                Image1 = model.Private ? Images.Locked : Images.Unlocked,
            //              Text2 = model.Language ?? "N/A",
            //                Image2 = Images.Language
            //            }));


            //Calculate the best representation of the size
            string size;
            if (model.Size / 1024f < 1)
                size = string.Format("{0:0.##}KB", model.Size);
            else if ((model.Size / 1024f / 1024f) < 1)
                size = string.Format("{0:0.##}MB", model.Size / 1024f);
            else
                size = string.Format("{0:0.##}GB", model.Size / 1024f / 1024f);

            //            sec1.Add(new SplitElement(new SplitElement.Row {
            //                Text1 = model.OpenIssues + (model.OpenIssues == 1 ? " Issue" : " Issues"),
            //                Image1 = Images.Flag,
            //                Text2 = model.Forks.ToString() + (model.Forks == 1 ? " Fork" : " Forks"),
            //                Image2 = Images.Fork
            //            }));
            //
            //            sec1.Add(new SplitElement(new SplitElement.Row {
            //                Text1 = (model.CreatedAt).ToString("MM/dd/yy"),
            //                Image1 = Images.Create,
            //                Text2 = size,
            //                Image2 = Images.Size
            //            }));

            var owner = new StyledStringElement("Owner", model.Owner.Login) { Image = Images.Person,  Accessory = UITableViewCellAccessory.DisclosureIndicator };
            owner.Tapped += () => ViewModel.GoToOwnerCommand.ExecuteIfCan();
            sec1.Add(owner);

            if (model.Parent != null)
            {
                var parent = new StyledStringElement("Forked From", model.Parent.FullName) { Image = Images.Fork,  Accessory = UITableViewCellAccessory.DisclosureIndicator };
                parent.Tapped += () => ViewModel.GoToForkParentCommand.Execute(model.Parent);
                sec1.Add(parent);
            }

            var followers = new StyledStringElement("Stargazers", "" + model.StargazersCount) { Image = Images.Star, Accessory = UITableViewCellAccessory.DisclosureIndicator };
            followers.Tapped += () => ViewModel.GoToStargazersCommand.ExecuteIfCan();
            sec1.Add(followers);

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

            root.Add(new[] { sec1, sec2, sec3 });

            if (!string.IsNullOrEmpty(model.Homepage))
            {
                var web = new StyledStringElement("Website", () => ViewModel.GoToUrlCommand.Execute(model.Homepage), Images.Webpage);
                root.Add(new Section { web });
            }

            Root = root;
        }

//        private void ForkRepository()
//        {
//            var repoModel = Controller.Model.RepositoryModel;
//            var alert = new UIAlertView();
//            alert.Title = "Fork";
//            alert.Message = "What would you like to name your fork?";
//            alert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
//            var forkButton = alert.AddButton("Fork!");
//            var cancelButton = alert.AddButton("Cancel");
//            alert.CancelButtonIndex = cancelButton;
//            alert.DismissWithClickedButtonIndex(cancelButton, true);
//            alert.GetTextField(0).Text = repoModel.Name;
//            alert.Clicked += (object sender2, UIButtonEventArgs e2) => {
//                if (e2.ButtonIndex == forkButton)
//                {
//                    var text = alert.GetTextField(0).Text;
//                    this.DoWork("Forking...", () => {
//                        //var fork = Application.Client.Users[model.Owner.Login].Repositories[model.Name].Fo(text);
//                        BeginInvokeOnMainThread(() => {
//                            //  NavigationController.PushViewController(new RepositoryInfoViewController(fork), true);
//                        });
//                    }, (ex) => {
//                        //We typically get a 'BAD REQUEST' but that usually means that a repo with that name already exists
//                        MonoTouch.Utilities.ShowAlert("Unable to fork", "A repository by that name may already exist in your collection or an internal error has occured.");
//                    });
//                }
//            };
//
//            alert.Show();
//        }
    }
}