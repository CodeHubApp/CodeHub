using CodeFramework.iOS.ViewControllers;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Organizations;
using MonoTouch.Dialog;
using UIKit;
using CoreGraphics;

namespace CodeHub.iOS.Views.Organizations
{
    public class OrganizationView : PrettyDialogViewController
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var vm = (OrganizationViewModel) ViewModel;

            HeaderView.SetImage(null, Images.Avatar);
            Title = vm.Name;
            HeaderView.Text = vm.Name;

            vm.Bind(x => x.Organization, x =>
            {
                HeaderView.SubText = string.IsNullOrWhiteSpace(x.Name) ? x.Login : x.Name;
                HeaderView.SetImage(x.AvatarUrl, Images.Avatar);
                RefreshHeaderView();
            });

            var members = new StyledStringElement("Members".t(), () => vm.GoToMembersCommand.Execute(null), Images.Following);
            var teams = new StyledStringElement("Teams".t(), () => vm.GoToTeamsCommand.Execute(null), Images.Team);
            var followers = new StyledStringElement("Followers".t(), () => vm.GoToFollowersCommand.Execute(null), Images.Heart);
            var events = new StyledStringElement("Events".t(), () => vm.GoToEventsCommand.Execute(null), Images.Event);
            var repos = new StyledStringElement("Repositories".t(), () => vm.GoToRepositoriesCommand.Execute(null), Images.Repo);
            var gists = new StyledStringElement("Gists", () => vm.GoToGistsCommand.Execute(null), Images.Script);
            Root = new RootElement(vm.Name) { new Section(new UIView(new CGRect(0, 0, 0, 20f))) { members, teams }, new Section { events, followers }, new Section { repos, gists } };
        }
    }
}

