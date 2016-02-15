using CodeHub.iOS.Views.User;

namespace CodeHub.iOS.Views.Teams
{
    public class TeamMembersView : BaseUserCollectionView
    {
        public override void ViewDidLoad()
        {
            Title = "Members";
            NoItemsText = "No Members";
            base.ViewDidLoad();
        }
    }
}