using Xamarin.Utilities.Core.ViewModels;
using CodeHub.Core.ViewModels;

namespace ReactiveUI
{
    public abstract class ReactiveTableViewController<TViewModel> : ReactiveTableViewController, IViewFor<TViewModel> where TViewModel : class
    {
        private TViewModel _viewModel;
        public TViewModel ViewModel
        {
            get { return _viewModel; }
            set { this.RaiseAndSetIfChanged(ref _viewModel, value); }
        }

        object IViewFor.ViewModel
        {
            get { return _viewModel; }
            set { ViewModel = (TViewModel)value; }
        }

        public override void LoadView()
        {
            base.LoadView();

            this.WhenActivated(d =>
            {
                // Do nothing. This allows the VM to get called.
            });

            CreateRefreshControl();
            CreateSearchBar();
            LoadViewModel();
        }

        protected virtual void LoadViewModel()
        {
            var iLoadableViewModel = ViewModel as ILoadableViewModel;
            if (iLoadableViewModel != null)
                iLoadableViewModel.LoadCommand.ExecuteIfCan();
        }

        protected virtual void CreateRefreshControl()
        {
            var iLoadableViewModel = ViewModel as ILoadableViewModel;
            if (iLoadableViewModel != null)
                RefreshControl = ((ILoadableViewModel)ViewModel).LoadCommand.ToRefreshControl();
        }

        protected virtual void CreateSearchBar()
        {
            var searchableViewModel = ViewModel as ISearchableViewModel;
            if (searchableViewModel != null)
                this.AddSearchBar(x => searchableViewModel.SearchKeyword = x);
        }
    }
}

