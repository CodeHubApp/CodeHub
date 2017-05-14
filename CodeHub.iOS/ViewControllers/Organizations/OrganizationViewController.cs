using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Organizations;
using UIKit;
using CoreGraphics;
using CodeHub.iOS.DialogElements;
using System;
using System.Reactive.Linq;
using CodeHub.iOS.ViewControllers.Users;
using CodeHub.iOS.ViewControllers.Gists;

namespace CodeHub.iOS.ViewControllers.Organizations
{
    public class OrganizationViewController : PrettyDialogViewController
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var vm = (OrganizationViewModel) ViewModel;

            HeaderView.SetImage(null, Images.Avatar);
            Title = vm.Name;
            HeaderView.Text = vm.Name;

            var members = new StringElement("Members", Octicon.Person.ToImage());
            var teams = new StringElement("Teams", Octicon.Organization.ToImage());
            var followers = new StringElement("Followers", Octicon.Heart.ToImage());
            var events = new StringElement("Events", Octicon.Rss.ToImage());
            var repos = new StringElement("Repositories", Octicon.Repo.ToImage());
            var gists = new StringElement("Gists", Octicon.Gist.ToImage());
            Root.Reset(new Section(new UIView(new CGRect(0, 0, 0, 20f))) { members, teams }, new Section { events, followers }, new Section { repos, gists });

            OnActivation(d =>
            {
                d(teams.Clicked.BindCommand(vm.GoToTeamsCommand));
                d(events.Clicked.BindCommand(vm.GoToEventsCommand));

                d(members.Clicked
                  .Select(_ => UsersViewController.CreateOrganizationMembersViewController(vm.Name))
                  .Subscribe(x => NavigationController.PushViewController(x, true)));
                
                d(followers.Clicked
                  .Select(_ => UsersViewController.CreateFollowersViewController(vm.Name))
                  .Subscribe(x => NavigationController.PushViewController(x, true)));

                d(repos.Clicked.Subscribe(_ => {
                    var vc = Repositories.RepositoriesViewController.CreateOrganizationViewController(vm.Name);
                    NavigationController?.PushViewController(vc, true);
                }));

                d(gists.Clicked
                  .Select(x => GistsViewController.CreateUserGistsViewController(vm.Name))
                  .Subscribe(x => NavigationController.PushViewController(x, true)));

                d(vm.Bind(x => x.Organization, true).Where(x => x != null).Subscribe(x =>
                {
                    HeaderView.SubText = string.IsNullOrWhiteSpace(x.Name) ? x.Login : x.Name;
                    HeaderView.SetImage(x.AvatarUrl, Images.Avatar);
                    RefreshHeaderView();
                }));
            });
        }
    }
}

