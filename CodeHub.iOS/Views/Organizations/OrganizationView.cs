using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Organizations;
using ReactiveUI;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.Organizations
{
    public class OrganizationView : BaseDialogViewController<OrganizationViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            HeaderView.SubText = "Organization";
            HeaderView.Image = Images.LoginUserUnknown;

            var split = new SplitButtonElement();
            var followers = split.AddButton("Followers", "-", ViewModel.GoToFollowersCommand.ExecuteIfCan);
            var following = split.AddButton("Following", "-", ViewModel.GoToFollowingCommand.ExecuteIfCan);
            var members = new DialogStringElement("Members", ViewModel.GoToMembersCommand.ExecuteIfCan, Images.Following);
            var teams = new DialogStringElement("Teams", ViewModel.GoToTeamsCommand.ExecuteIfCan, Images.Organization);
            var events = new DialogStringElement("Events", ViewModel.GoToEventsCommand.ExecuteIfCan, Images.Rss);
            var repos = new DialogStringElement("Repositories", ViewModel.GoToRepositoriesCommand.ExecuteIfCan, Images.Repo);
            var gists = new DialogStringElement("Gists", ViewModel.GoToGistsCommand.ExecuteIfCan, Images.Gist);
            Root.Reset(new Section { split }, new Section { members, teams }, new Section { events }, new Section { repos, gists });

            this.WhenAnyValue(x => x.ViewModel.Organization)
                .IsNotNull()
                .Subscribe(x =>
                {
                    followers.Text = x != null ? x.Followers.ToString() : "-";
                    following.Text = x != null ? x.Following.ToString() : "-";
                    HeaderView.ImageUri = x.AvatarUrl;
                    TableView.ReloadData();
                });
        }
    }
}

