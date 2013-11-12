namespace CodeHub.iOS.Views.User
{
    public class TeamMembersView : BaseUserCollectionView
    {
        public override void ViewDidLoad()
        {
            Title = "Members";
            SearchPlaceholder = "Search Members".t();
            NoItemsText = "No Members".t();

            base.ViewDidLoad();
        }
    }
}