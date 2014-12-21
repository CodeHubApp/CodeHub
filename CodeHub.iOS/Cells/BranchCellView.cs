using System;
using System.Reactive.Linq;
using MonoTouch.Foundation;
using CodeHub.Core.ViewModels.Source;

namespace CodeHub.iOS.Cells
{
    public class BranchCellView : ReactiveTableViewCell<BranchItemViewModel>
    {
        public static NSString Key = new NSString("BranchCell");

        public BranchCellView(IntPtr handle)
            : base(handle)
        {
            this.WhenViewModel(x => x.Name).Subscribe(x => TextLabel.Text = x);
        }
    }
}

