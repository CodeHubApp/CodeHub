using CodeFramework.iOS.ViewControllers;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Organizations;
using MonoTouch.Dialog;

namespace CodeHub.iOS.Views.Organizations
{
    public class OrganizationView : ViewModelDrivenDialogViewController
    {
        private HeaderView _header;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var vm = (OrganizationViewModel) ViewModel;
            _header = new HeaderView() { Title = vm.Name };

            vm.Bind(x => x.Organization, model =>
            {
                _header.Subtitle = string.IsNullOrEmpty(model.Name) ? model.Login : model.Name;
				_header.ImageUri = model.AvatarUrl;
            });

            var members = new StyledStringElement("Members".t(), () => vm.GoToMembersCommand.Execute(null), Images.Following);
            var teams = new StyledStringElement("Teams".t(), () => vm.GoToTeamsCommand.Execute(null), Images.Team);
            var followers = new StyledStringElement("Followers".t(), () => vm.GoToFollowersCommand.Execute(null), Images.Heart);
            var events = new StyledStringElement("Events".t(), () => vm.GoToEventsCommand.Execute(null), Images.Event);
            var repos = new StyledStringElement("Repositories".t(), () => vm.GoToRepositoriesCommand.Execute(null), Images.Repo);
            var gists = new StyledStringElement("Gists", () => vm.GoToGistsCommand.Execute(null), Images.Script);
			Root = new RootElement(vm.Name) { new Section(_header), new Section { members, teams }, new Section { events, followers }, new Section { repos, gists } };
        }
    }
}

