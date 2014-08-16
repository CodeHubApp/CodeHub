using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Organizations;
using ReactiveUI;
using Xamarin.Utilities.DialogElements;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Organizations
{
    public class OrganizationView : ViewModelPrettyDialogViewController<OrganizationViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = ViewModel.Name;

            ViewModel.WhenAnyValue(x => x.Organization).Where(x => x != null).Subscribe(x =>
            {
                Title = HeaderView.Text = string.IsNullOrEmpty(x.Name) ? x.Login : x.Name;
                HeaderView.ImageUri = x.AvatarUrl;
                ReloadData();
            });

            var members = new StyledStringElement("Members", ViewModel.GoToMembersCommand.ExecuteIfCan, Images.Following);
            var teams = new StyledStringElement("Teams", ViewModel.GoToTeamsCommand.ExecuteIfCan, Images.Team);
            var followers = new StyledStringElement("Followers", ViewModel.GoToFollowersCommand.ExecuteIfCan, Images.Heart);
            var events = new StyledStringElement("Events", ViewModel.GoToEventsCommand.ExecuteIfCan, Images.Event);
            var repos = new StyledStringElement("Repositories", ViewModel.GoToRepositoriesCommand.ExecuteIfCan, Images.Repo);
            var gists = new StyledStringElement("Gists", ViewModel.GoToGistsCommand.ExecuteIfCan, Images.Script);
            Root.Reset(new Section(HeaderView), new Section { members, teams }, new Section { events, followers }, new Section { repos, gists });
        }
    }
}

