using System;
using ReactiveUI;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using Foundation;
using System.Reactive;

namespace CodeHub.iOS.ViewControllers
{
    public abstract class BaseViewController : ReactiveViewController, IActivatable
    {
        private readonly ISubject<bool> _appearingSubject = new Subject<bool>();
        private readonly ISubject<bool> _appearedSubject = new Subject<bool>();
        private readonly ISubject<bool> _disappearingSubject = new Subject<bool>();
        private readonly ISubject<bool> _disappearedSubject = new Subject<bool>();
        private readonly ISubject<Unit> _loadedSubject = new Subject<Unit>();

#if DEBUG
        ~BaseViewController()
        {
            Console.WriteLine("All done with " + GetType().Name);
        }
#endif

        public IObservable<Unit> Loaded => _loadedSubject.AsObservable();

        public IObservable<bool> Appearing => _appearingSubject.AsObservable();

        public IObservable<bool> Appeared => _appearedSubject.AsObservable();

        public IObservable<bool> Disappearing => _disappearingSubject.AsObservable();

        public IObservable<bool> Disappeared => _disappearedSubject.AsObservable();

        public void OnActivation(Action<Action<IDisposable>> d)
        {
            this.WhenActivated(d);
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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _loadedSubject.OnNext(Unit.Default);
        }
    }
}

