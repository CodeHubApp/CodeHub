using CodeHub.iOS.Views.User;
using CodeHub.Core.ViewModels.User;

namespace CodeHub.iOS.Views.Teams
{
    public class TeamMembersView : BaseUserCollectionView<TeamMembersViewModel>
    {
        public override void ViewDidLoad()
        {
            Title = "Members";
            NoItemsText = "No Members";

            base.ViewDidLoad();
        }
    }
}