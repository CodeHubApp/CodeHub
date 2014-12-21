using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Organizations;
using ReactiveUI;
using Xamarin.Utilities.DialogElements;

namespace CodeHub.iOS.Views.Organizations
{
    public class OrganizationView : ReactiveDialogViewController<OrganizationViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            HeaderView.SubText = "Organization";

            var split = new SplitButtonElement();
            var followers = split.AddButton("Followers", "-", () => ViewModel.GoToFollowersCommand.ExecuteIfCan());
            var following = split.AddButton("Following", "-", () => ViewModel.GoToFollowingCommand.ExecuteIfCan());
            var members = new StyledStringElement("Members", ViewModel.GoToMembersCommand.ExecuteIfCan, Images.Following);
            var teams = new StyledStringElement("Teams", ViewModel.GoToTeamsCommand.ExecuteIfCan, Images.Team);
            var events = new StyledStringElement("Events", ViewModel.GoToEventsCommand.ExecuteIfCan, Images.Event);
            var repos = new StyledStringElement("Repositories", ViewModel.GoToRepositoriesCommand.ExecuteIfCan, Images.Repo);
            var gists = new StyledStringElement("Gists", ViewModel.GoToGistsCommand.ExecuteIfCan, Images.Script);
            Root.Reset(new Section { split }, new Section { members, teams }, new Section { events }, new Section { repos, gists });

            ViewModel.WhenAnyValue(x => x.Organization).Where(x => x != null).Subscribe(x =>
            {
                followers.Text = x.Followers.ToString();
                following.Text = x.Following.ToString();
                HeaderView.ImageUri = x.AvatarUrl;
                TableView.ReloadData();
            });
        }
    }
}

