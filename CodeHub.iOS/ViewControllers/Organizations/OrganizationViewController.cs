using System;
using System.Reactive.Linq;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.ViewControllers.Gists;
using CodeHub.iOS.ViewControllers.Repositories;
using CodeHub.iOS.ViewControllers.Users;
using CoreGraphics;
using ReactiveUI;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Organizations
{
    public class OrganizationViewController : ItemDetailsViewController
    {
        public string OrgName { get; }

        private Octokit.Organization _organization;
        public Octokit.Organization Organization
        {
            get { return _organization; }
            private set { this.RaiseAndSetIfChanged(ref _organization, value); }
        }

        public OrganizationViewController(Octokit.Organization org)
            : this(org.Login)
        {
            Organization = org;
        }

        public OrganizationViewController(string org)
        {
            OrgName = org;
            Title = org;
            HeaderView.Text = org;
            HeaderView.SetImage(null, Images.Avatar);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var members = new StringElement("Members", Octicon.Person.ToImage());
            var teams = new StringElement("Teams", Octicon.Organization.ToImage());
            var followers = new StringElement("Followers", Octicon.Heart.ToImage());
            var events = new StringElement("Events", Octicon.Rss.ToImage());
            var repos = new StringElement("Repositories", Octicon.Repo.ToImage());
            var gists = new StringElement("Gists", Octicon.Gist.ToImage());

            Root.Reset(
                new Section(new UIView(new CGRect(0, 0, 0, 20f))) { members, teams },
                new Section { events, followers },
                new Section { repos, gists });

            teams.Clicked
              .Select(_ => TeamsViewController.OrganizationTeams(OrgName))
              .Subscribe(this.PushViewController);

            //d(events.Clicked.BindCommand(vm.GoToEventsCommand));

            members.Clicked
              .Select(_ => UsersViewController.CreateOrganizationMembersViewController(OrgName))
              .Subscribe(this.PushViewController);

            followers.Clicked
              .Select(_ => UsersViewController.CreateFollowersViewController(OrgName))
              .Subscribe(this.PushViewController);

            repos.Clicked
              .Select(_ => RepositoriesViewController.CreateOrganizationViewController(OrgName))
              .Subscribe(this.PushViewController);

            gists.Clicked
              .Select(x => GistsViewController.CreateUserGistsViewController(OrgName))
              .Subscribe(this.PushViewController);

            this.WhenAnyValue(x => x.Organization).Where(x => x != null).Subscribe(x =>
            {
                HeaderView.SubText = string.IsNullOrWhiteSpace(x.Name) ? x.Login : x.Name;
                HeaderView.SetImage(x.AvatarUrl, Images.Avatar);
                RefreshHeaderView();
            });
        }
    }
}

