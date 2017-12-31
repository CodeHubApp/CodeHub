using CodeHub.Core.ViewModels.Changesets;

namespace CodeHub.iOS.Views.Source
{
    public class ChangesetsView : CommitsView
    {
        public ChangesetsView()
            : base()
        {
        }

        public ChangesetsView(string owner, string repository, string branch)
            : base()
        {
            var viewModel = new ChangesetsViewModel();
            viewModel.Init(new ChangesetsViewModel.NavObject
            {
                Username = owner,
                Repository = repository,
                Branch = branch
            });

            ViewModel = viewModel;
        }
    }
}
