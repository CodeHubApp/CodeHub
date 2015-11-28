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
            var followers = split.AddButton("Followers", "-");
            var following = split.AddButton("Following", "-");
            var members = new StringElement("Members", Octicon.Person.ToImage());
            var teams = new StringElement("Teams", Octicon.Organization.ToImage());
            var events = new StringElement("Events", Octicon.Rss.ToImage());
            var repos = new StringElement("Repositories", Octicon.Repo.ToImage());
            var gists = new StringElement("Gists", Octicon.Gist.ToImage());
            var membersAndTeams = new Section { members };

            Root.Reset(new Section { split }, membersAndTeams, new Section { events }, new Section { repos, gists });

            OnActivation(d => {
                d(followers.Clicked.InvokeCommand(ViewModel.GoToFollowersCommand));
                d(following.Clicked.InvokeCommand(ViewModel.GoToFollowingCommand));

                d(members.Clicked.InvokeCommand(ViewModel.GoToMembersCommand));
                d(teams.Clicked.InvokeCommand(ViewModel.GoToTeamsCommand));
                d(events.Clicked.InvokeCommand(ViewModel.GoToEventsCommand));
                d(repos.Clicked.InvokeCommand(ViewModel.GoToRepositoriesCommand));
                d(gists.Clicked.InvokeCommand(ViewModel.GoToGistsCommand));

                d(this.WhenAnyValue(x => x.ViewModel.Avatar)
                    .Subscribe(x => HeaderView.SetImage(x?.ToUri(128), Images.LoginUserUnknown)));

                var orgObs = this.WhenAnyValue(x => x.ViewModel.Organization);
                d(followers.BindText(orgObs.Select(x => x != null ? x.Followers.ToString() : "-")));
                d(following.BindText(orgObs.Select(x => x != null ? x.Following.ToString() : "-")));

                d(this.WhenAnyValue(x => x.ViewModel.CanViewTeams)
                    .Where(x => x && teams.Section == null)
                    .Subscribe(x => membersAndTeams.Add(teams)));

                d(this.WhenAnyValue(x => x.ViewModel.CanViewTeams)
                    .Where(x => !x)
                    .Subscribe(x => membersAndTeams.Remove(teams)));
            });
        }
    }
}

