using CodeHub.Core.ViewModels.Repositories;
using CodeHub.iOS.ViewControllers.Users;

namespace CodeHub.iOS.ViewControllers.Repositories
{
    public class RepositoryCollaboratorsViewController : BaseUserCollectionViewController<RepositoryCollaboratorsViewModel>
    {
        public RepositoryCollaboratorsViewController()
            : base("There are no collaborators.")
        {
        }
    }
}

