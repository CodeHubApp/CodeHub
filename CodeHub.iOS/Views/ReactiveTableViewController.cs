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
        }
    }
}

