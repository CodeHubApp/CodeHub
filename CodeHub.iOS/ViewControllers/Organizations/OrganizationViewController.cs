using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Organizations;
using ReactiveUI;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.ViewControllers.Organizations
{
    public class OrganizationViewController : BaseDialogViewController<OrganizationViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            HeaderView.SubText = "Organization";
            HeaderView.Image = Images.LoginUserUnknown;

            var split = new SplitButtonElement();
            var followers = split.AddButton("Followers", "-", () => ViewModel.GoToFollowersCommand.ExecuteIfCan());
            var following = split.AddButton("Following", "-", () => ViewModel.GoToFollowingCommand.ExecuteIfCan());
            var members = new StringElement("Members", () => ViewModel.GoToMembersCommand.ExecuteIfCan(), Octicon.Person.ToImage());
            var teams = new StringElement("Teams", () => ViewModel.GoToTeamsCommand.ExecuteIfCan(), Octicon.Organization.ToImage());
            var events = new StringElement("Events", () => ViewModel.GoToEventsCommand.ExecuteIfCan(), Octicon.Rss.ToImage());
            var repos = new StringElement("Repositories", () => ViewModel.GoToRepositoriesCommand.ExecuteIfCan(), Octicon.Repo.ToImage());
            var gists = new StringElement("Gists", () => ViewModel.GoToGistsCommand.ExecuteIfCan(), Octicon.Gist.ToImage());
            var membersAndTeams = new Section { members };

            Root.Reset(new Section { split }, membersAndTeams, new Section { events }, new Section { repos, gists });

            this.WhenAnyValue(x => x.ViewModel.Avatar)
                .Subscribe(x => HeaderView.SetImage(x?.ToUri(128), Images.LoginUserUnknown));

            this.WhenAnyValue(x => x.ViewModel.Organization)
                .IsNotNull()
                .Subscribe(x => {
                    followers.Text = x != null ? x.Followers.ToString() : "-";
                    following.Text = x != null ? x.Following.ToString() : "-";
                });

            this.WhenAnyValue(x => x.ViewModel.CanViewTeams)
                .Where(x => x && teams.Section == null)
                .Subscribe(x => membersAndTeams.Add(teams));

            this.WhenAnyValue(x => x.ViewModel.CanViewTeams)
                .Where(x => !x)
                .Subscribe(x => membersAndTeams.Remove(teams));
        }
    }
}

