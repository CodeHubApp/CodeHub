using System;
using System.Reactive.Linq;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.ViewControllers.Users;
using Octokit;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Organizations
{
    public class TeamsViewController : GitHubListViewController<Team>
    {
        public static TeamsViewController OrganizationTeams(string org)
            => new TeamsViewController(ApiUrls.OrganizationTeams(org));

        private TeamsViewController(Uri uri) : base(uri, Octicon.Organization)
        {
            Title = "Teams";
        }

        protected override Element ConvertToElement(Team item)
        {
            var e = new StringElement(item.Name);
            e.Clicked
             .Select(_ => UsersViewController.CreateTeamMembersViewController(item.Id))
             .Subscribe(this.PushViewController);
            return e;
        }
    }
}