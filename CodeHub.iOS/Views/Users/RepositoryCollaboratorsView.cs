using CodeHub.Core.ViewModels.Users;

namespace CodeHub.iOS.Views.Users
{
    public class RepositoryCollaboratorsView : BaseUserCollectionView<RepositoryCollaboratorsViewModel>
    {
        public RepositoryCollaboratorsView()
            : base("There are no collaborators.")
        {
        }
    }
}

