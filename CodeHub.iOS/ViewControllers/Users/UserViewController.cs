using CodeHub.Core.ViewModels.User;
using UIKit;
using System;
using CodeHub.iOS.DialogElements;
using System.Reactive.Linq;
using CodeHub.iOS.ViewControllers.Gists;
using ReactiveUI;

namespace CodeHub.iOS.ViewControllers.Users
{
    public class UserViewController : PrettyDialogViewController
    {
        private readonly Lazy<UIBarButtonItem> _actionButton;

        public new UserViewModel ViewModel
        {
            get { return (UserViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public UserViewController(Octokit.User user)
            : this(user.Login, user)
        {
        }

        public UserViewController(
            string username,
            Octokit.User user = null)
        {
            ViewModel = new UserViewModel(username) { User = user };

            _actionButton = new Lazy<UIBarButtonItem>(() =>
                new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu()));
        }
            
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            HeaderView.SetImage(null, Images.Avatar);
            HeaderView.Text = ViewModel.Username;

            var split = new SplitButtonElement();
            var followers = split.AddButton("Followers", "-");
            var following = split.AddButton("Following", "-");

            var events = new StringElement("Events", Octicon.Rss.ToImage());
            var organizations = new StringElement("Organizations", Octicon.Organization.ToImage());
            var repos = new StringElement("Repositories", Octicon.Repo.ToImage());
            var gists = new StringElement("Gists", Octicon.Gist.ToImage());
            Root.Add(new [] { new Section { split }, new Section { events, organizations, repos, gists } });

            ViewModel.WhenAnyValue(x => x.User).Subscribe(x =>
            {
                followers.Text = x?.Followers.ToString() ?? "-";
                following.Text = x?.Following.ToString() ?? "-";
                HeaderView.SubText = string.IsNullOrWhiteSpace(x?.Name) ? null : x.Name;
                HeaderView.SetImage(x?.AvatarUrl, Images.Avatar);
                RefreshHeaderView();
            });

            OnActivation(d =>
            {
                d(followers.Clicked
                  .Select(x => UsersViewController.CreateFollowersViewController(ViewModel.Username))
                  .Subscribe(x => NavigationController.PushViewController(x, true)));

                d(following.Clicked
                  .Select(x => UsersViewController.CreateFollowingViewController(ViewModel.Username))
                  .Subscribe(x => NavigationController.PushViewController(x, true)));
               
                //d(events.Clicked.BindCommand(ViewModel.GoToEventsCommand));
                //d(organizations.Clicked.BindCommand(ViewModel.GoToOrganizationsCommand));

                d(gists.Clicked
                  .Select(x => GistsViewController.CreateUserGistsViewController(ViewModel.Username))
                  .Subscribe(x => NavigationController.PushViewController(x, true)));

                d(ViewModel.WhenAnyValue(x => x.Title).Subscribe(x => Title = x));

                d(repos.Clicked.Subscribe(_ =>
                {
                    var vc = Repositories.RepositoriesViewController.CreateUserViewController(ViewModel.Username);
                    NavigationController?.PushViewController(vc, true);
                }));
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (!ViewModel.IsLoggedInUser)
                NavigationItem.RightBarButtonItem = _actionButton.Value;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            NavigationItem.RightBarButtonItem = null;
        }

        private void ShowExtraMenu()
        {
            var sheet = new UIActionSheet();
            var followButton = sheet.AddButton(ViewModel.IsFollowing ? "Unfollow" : "Follow");
            var cancelButton = sheet.AddButton("Cancel");
            sheet.CancelButtonIndex = cancelButton;
            sheet.Dismissed += (s, e) => {
                BeginInvokeOnMainThread(() =>
                {
                    if (e.ButtonIndex == followButton)
                    {
                        ViewModel.ToggleFollowingCommand.ExecuteNow();
                    }
                });

                sheet.Dispose();
            };

            sheet.ShowInView(this.View);
        }
    }
}

