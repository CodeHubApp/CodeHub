using System;
using ReactiveUI;
using System.Reactive.Subjects;
using System.Reactive;

namespace CodeHub.iOS.Cells
{
    public abstract class ReactiveTableViewCell<TViewModel> : ReactiveTableViewCell, IViewFor<TViewModel> where TViewModel : class
    {
        private readonly ISubject<Unit> _appearingSubject = new Subject<Unit>();

        ~ReactiveTableViewCell()
        {
            Console.WriteLine("Goodbye " + this.GetType().Name);
        }

        public IObservable<Unit> Appearing
        {
            get { return _appearingSubject; }
        }

        protected ReactiveTableViewCell()
        {
        }

        protected ReactiveTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        protected ReactiveTableViewCell(UIKit.UITableViewCellStyle style, Foundation.NSString reuseIdentifier) 
            : base(style, reuseIdentifier)
        {
        }

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
    }
}

