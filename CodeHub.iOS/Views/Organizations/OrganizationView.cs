using System;
using System.Reactive.Linq;
using CodeFramework.iOS.ViewComponents;
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

            var header = new HeaderView { Title = ViewModel.Name };
            ViewModel.WhenAnyValue(x => x.Organization).Where(x => x != null).Subscribe(x =>
            {
                header.Subtitle = string.IsNullOrEmpty(x.Name) ? x.Login : x.Name;
                header.ImageUri = x.AvatarUrl;
            });

            var members = new StyledStringElement("Members", () => ViewModel.GoToMembersCommand.Execute(null), Images.Following);
            var teams = new StyledStringElement("Teams", () => ViewModel.GoToTeamsCommand.Execute(null), Images.Team);
            var followers = new StyledStringElement("Followers", () => ViewModel.GoToFollowersCommand.Execute(null), Images.Heart);
            var events = new StyledStringElement("Events", () => ViewModel.GoToEventsCommand.Execute(null), Images.Event);
            var repos = new StyledStringElement("Repositories", () => ViewModel.GoToRepositoriesCommand.Execute(null), Images.Repo);
            var gists = new StyledStringElement("Gists", () => ViewModel.GoToGistsCommand.Execute(null), Images.Script);
            Root.Reset(new Section(header), new Section { members, teams }, new Section { events, followers }, new Section { repos, gists });
        }
    }
}

