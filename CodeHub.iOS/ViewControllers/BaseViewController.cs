using System;
using ReactiveUI;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Collections.Generic;
using Foundation;

namespace CodeHub.iOS.ViewControllers
{
    public abstract class BaseViewController : ReactiveViewController, IActivatable
    {
        private readonly ISubject<bool> _appearingSubject = new Subject<bool>();
        private readonly ISubject<bool> _appearedSubject = new Subject<bool>();
        private readonly ISubject<bool> _disappearingSubject = new Subject<bool>();
        private readonly ISubject<bool> _disappearedSubject = new Subject<bool>();
        private readonly ICollection<IDisposable> _activations = new LinkedList<IDisposable>();

        #if DEBUG
        ~BaseViewController()
        {
            Console.WriteLine("All done with " + GetType().Name);
        }
        #endif

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
            this.WhenActivated(_ => { });
        }

        private void DisposeActivations()
        {
            foreach (var a in _activations)
                a.Dispose();
            _activations.Clear();
        }

        public override void ViewWillAppear(bool animated)
        {
            MvvmCross.Platform.Mvx.Resolve<CodeHub.Core.Services.IAnalyticsService>()
                .LogScreen(GetType().Name);

            base.ViewWillAppear(animated);
            DisposeActivations();
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
            DisposeActivations();
            _disappearingSubject.OnNext(animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _disappearedSubject.OnNext(animated);
        }
    }
}

