using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Repositories
{
    public abstract class RepositoriesViewModel : LoadableViewModel
    {
        public bool ShowRepositoryDescription
        {
            get { return this.GetApplication().Account.ShowRepositoryDescriptionInList; }
        }

        public CollectionViewModel<RepositoryModel> Repositories { get; }

        public bool ShowRepositoryOwner { get; protected set; }

        public ICommand GoToRepositoryCommand
        {
            get { return new MvxCommand<RepositoryModel>(x => this.ShowViewModel<RepositoryViewModel>(new RepositoryViewModel.NavObject { Username = x.Owner.Login, Repository = x.Name })); }
        }

        protected RepositoriesViewModel()
        {
            Repositories = new CollectionViewModel<RepositoryModel>();
        }
    }
}