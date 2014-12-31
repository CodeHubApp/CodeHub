using System;
using ReactiveUI;
using System.Reactive.Linq;
using Xamarin.Utilities.ViewModels;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Views
{
    public abstract class BaseViewController<TViewModel> : ReactiveViewController, IViewFor<TViewModel> where TViewModel : class
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

        protected BaseViewController()
        {
            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = string.Empty };
            this.WhenAnyValue(x => x.ViewModel).OfType<ILoadableViewModel>().Subscribe(x => x.LoadCommand.ExecuteIfCan());
            this.WhenAnyValue(x => x.ViewModel).OfType<IProvidesTitle>().Select(x => x.WhenAnyValue(y => y.Title)).Switch().Subscribe(x => Title = x);
            this.WhenActivated(d => { });
        }
    }

    public abstract class BaseViewController : ReactiveViewController
    {
    }
}

