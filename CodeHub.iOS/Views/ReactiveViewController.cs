using System;
using ReactiveUI;
using MonoTouch.UIKit;
using Xamarin.Utilities.Core;
using Xamarin.Utilities.Core.ViewModels;
using MonoTouch.Foundation;

namespace CodeHub.iOS.Views
{
    public abstract class ReactiveViewController<TViewModel> : ReactiveViewController, IViewFor<TViewModel> where TViewModel : class
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

        protected ReactiveViewController(string nibNameOrNull, NSBundle nibBundleOrNull) 
            : base(nibNameOrNull, nibBundleOrNull)
        {
        }

        protected ReactiveViewController(IntPtr handle) 
            : base(handle)
        {
        }

        protected ReactiveViewController()
        {
        }

        public override void LoadView()
        {
            base.LoadView();

            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = string.Empty };

            this.WhenActivated(d =>
            {
                // Always keep this around since it calls the VM WhenActivated
            });

            var providesTitle = ViewModel as IProvidesTitle;
            if (providesTitle != null)
                providesTitle.WhenAnyValue(x => x.Title).Subscribe(x => Title = x ?? string.Empty);

            var iLoadableViewModel = ViewModel as ILoadableViewModel;
            if (iLoadableViewModel != null)
                iLoadableViewModel.LoadCommand.ExecuteIfCan();
        }
    }
}

