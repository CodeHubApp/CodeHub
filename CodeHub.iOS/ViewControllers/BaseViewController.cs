using System;
using ReactiveUI;
using System.Reactive.Linq;
using UIKit;
using System.Reactive.Subjects;
using Foundation;
using CodeHub.Core.ViewModels;
using Splat;
using CodeHub.Core.Services;
using CodeHub.iOS.ViewControllers;
using System.Collections.Generic;

namespace CodeHub.iOS.ViewControllers
{
    public abstract class BaseViewController<TViewModel> : BaseViewController, IViewFor<TViewModel> where TViewModel : class
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
            SetupRx();
        }

        protected BaseViewController(string nib, NSBundle bundle)
            : base(nib, bundle)
        {
            SetupRx();
        }

        private void SetupRx()
        {
            OnActivation(d => {
                d(this.WhenAnyValue(x => x.ViewModel)
                    .OfType<IProvidesTitle>()
                    .Select(x => x.WhenAnyValue(y => y.Title))
                    .Switch().Subscribe(x => Title = x ?? string.Empty));

                d(this.WhenAnyValue(x => x.ViewModel)
                    .OfType<IRoutingViewModel>()
                    .Select(x => x.RequestNavigation)
                    .Switch()
                    .Subscribe(x => {
                        var viewModelViewService = Locator.Current.GetService<IViewModelViewService>();
                        var serviceConstructor = Locator.Current.GetService<IServiceConstructor>();
                        var viewType = viewModelViewService.GetViewFor(x.GetType());
                        var view = (IViewFor)serviceConstructor.Construct(viewType);
                        view.ViewModel = x;
                        HandleNavigation(x, view as UIViewController);
                    }));
            });
        }

        protected virtual void HandleNavigation(IBaseViewModel viewModel, UIViewController view)
        {
            if (view is IModalViewController)
            {
                PresentViewController(new ThemedNavigationController(view), true, null);
                viewModel.RequestDismiss.Subscribe(_ => DismissViewController(true, null));
            }
            else
            {
                NavigationController.PushViewController(view, true);
                viewModel.RequestDismiss.Subscribe(_ => NavigationController.PopToViewController(this, true));
            }
        }
    }

    public abstract class BaseViewController : ReactiveViewController, IActivatable
    {
        private readonly ISubject<bool> _appearingSubject = new Subject<bool>();
        private readonly ISubject<bool> _appearedSubject = new Subject<bool>();
        private readonly ISubject<bool> _disappearingSubject = new Subject<bool>();
        private readonly ISubject<bool> _disappearedSubject = new Subject<bool>();
        private readonly ICollection<IDisposable> _activations = new LinkedList<IDisposable>();

        ~BaseViewController()
        {
            Console.WriteLine("All done with " + GetType().Name);
        }

        public IObservable<bool> Appearing
        {
            get { return _appearingSubject.AsObservable(); }
        }

        public IObservable<bool> Appeared
        {
            get { return _appearedSubject.AsObservable(); }
        }

        public IObservable<bool> Disappearing
        {
            get { return _disappearingSubject.AsObservable(); }
        }

        public IObservable<bool> Disappeared
        {
            get { return _disappearedSubject.AsObservable(); }
        }

        public void OnActivation(Action<Action<IDisposable>> d)
        {
            Appearing.Subscribe(_ => d(x => _activations.Add(x)));
        }

        protected BaseViewController()
        {
            CommonConstructor();
        }

        protected BaseViewController(string nib, NSBundle bundle)
            : base(nib, bundle)
        {
            CommonConstructor();
        }

        private void CommonConstructor()
        {
            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = string.Empty };
            Disappeared.Subscribe(_ => {
                foreach (var a in _activations)
                    a.Dispose();
                _activations.Clear();
            });
            this.WhenActivated(_ => { });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            _appearingSubject.OnNext(animated);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            _appearedSubject.OnNext(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            _disappearingSubject.OnNext(animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _disappearedSubject.OnNext(animated);
        }
    }
}

