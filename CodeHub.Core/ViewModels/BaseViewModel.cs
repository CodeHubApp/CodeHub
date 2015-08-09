using System;
using System.Reactive.Subjects;
using System.Reactive;
using ReactiveUI;

namespace CodeHub.Core.ViewModels
{
    public abstract class BaseViewModel : ReactiveObject, IBaseViewModel
    {
        private readonly ViewModelActivator _viewModelActivator = new ViewModelActivator();
        private readonly ISubject<IBaseViewModel> _requestNavigationSubject = new Subject<IBaseViewModel>();
        private readonly ISubject<Unit> _requestDismissSubject = new Subject<Unit>();

        private string _title;
        public string Title
        {
            get { return _title; }
            protected set { this.RaiseAndSetIfChanged(ref _title, value); }
        }

        protected void NavigateTo(IBaseViewModel viewModel)
        {
            var loadableViewModel = viewModel as ILoadableViewModel;
            _requestNavigationSubject.OnNext(viewModel);
            loadableViewModel?.LoadCommand.ExecuteIfCan();
        }

        protected void Dismiss()
        {
            _requestDismissSubject.OnNext(Unit.Default);
        }

        ViewModelActivator ISupportsActivation.Activator
        {
            get { return _viewModelActivator; }
        }

        IObservable<IBaseViewModel> IRoutingViewModel.RequestNavigation
        {
            get { return _requestNavigationSubject; }
        }

        IObservable<Unit> IRoutingViewModel.RequestDismiss
        {
            get { return _requestDismissSubject; }
        }
    }
}