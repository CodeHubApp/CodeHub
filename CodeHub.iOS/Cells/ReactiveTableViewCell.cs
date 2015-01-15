using System;
using ReactiveUI;

namespace CodeHub.iOS.Cells
{
    public abstract class ReactiveTableViewCell<TViewModel> : ReactiveTableViewCell, IViewFor<TViewModel> where TViewModel : class
    {
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

