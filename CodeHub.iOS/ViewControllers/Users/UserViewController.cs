using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Users;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.ViewControllers.Users
{
    public class UserViewController : BaseDialogViewController<UserViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            HeaderView.Image = Images.LoginUserUnknown;
            HeaderView.SubImageView.TintColor = UIColor.FromRGB(243, 156, 18);

            Appeared.Take(1)
                .Select(_ => Observable.Timer(TimeSpan.FromSeconds(0.35f)))
                .Switch()
                .Select(_ => this.WhenAnyValue(x => x.ViewModel.IsFollowing).Where(x => x.HasValue))
                .Switch()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => HeaderView.SetSubImage(x.Value ? Octicon.Star.ToImage() : null));

            var websiteElement = new StringElement("Website", Octicon.Globe.ToImage());
            var extraSection = new Section();
            var split = new SplitButtonElement();
            var followers = split.AddButton("Followers", "-");
            var following = split.AddButton("Following", "-");
            var events = new StringElement("Events", Octicon.Rss.ToImage());
            var organizations = new StringElement("Organizations", Octicon.Organization.ToImage());
            var repos = new StringElement("Repositories", Octicon.Repo.ToImage());
            var gists = new StringElement("Gists", Octicon.Gist.ToImage());
            Root.Reset(new [] { new Section { split }, new Section { events, organizations, repos, gists }, extraSection });

            OnActivation(d => {
                d(followers.Clicked.InvokeCommand(ViewModel.GoToFollowersCommand));
                d(following.Clicked.InvokeCommand(ViewModel.GoToFollowingCommand));
                d(websiteElement.Clicked.InvokeCommand(ViewModel.GoToWebsiteCommand));
                d(events.Clicked.InvokeCommand(ViewModel.GoToEventsCommand));
                d(organizations.Clicked.InvokeCommand(ViewModel.GoToOrganizationsCommand));
                d(repos.Clicked.InvokeCommand(ViewModel.GoToRepositoriesCommand));
                d(gists.Clicked.InvokeCommand(ViewModel.GoToGistsCommand));

                d(this.WhenAnyValue(x => x.ViewModel)
                    .Where(x => !x.IsLoggedInUser)
                    .Select(x => x.ShowMenuCommand)
                    .ToBarButtonItem(UIBarButtonSystemItem.Action, x => NavigationItem.RightBarButtonItem = x));

                d(this.WhenAnyValue(x => x.ViewModel.Avatar)
                    .Subscribe(x => HeaderView.SetImage(x?.ToUri(128), Images.LoginUserUnknown)));

                d(this.WhenAnyValue(x => x.ViewModel.User).IsNotNull().Subscribe(x => {
                    followers.Text = x.Followers.ToString();
                    following.Text = x.Following.ToString();
                    HeaderView.SubText = string.IsNullOrEmpty(x.Name) ? null : x.Name;
                    RefreshHeaderView();
                }));

                d(this.WhenAnyValue(x => x.ViewModel.HasBlog).Subscribe(x => {
                    if (x && websiteElement.Section == null)
                        extraSection.Add(websiteElement);
                    else if (!x && websiteElement.Section != null)
                        extraSection.Remove(websiteElement);
                }));
            });
        }
    }
}

