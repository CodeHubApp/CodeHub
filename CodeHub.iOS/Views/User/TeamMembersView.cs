namespace CodeHub.iOS.Views.User
{
    public class TeamMembersView : BaseUserCollectionView
    {
        public override void ViewDidLoad()
        {
            Title = "Members";
            NoItemsText = "No Members".t();

            base.ViewDidLoad();
        }
    }
}