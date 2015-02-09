using System;
using Foundation;
using CodeHub.Core.ViewModels.Source;
using ReactiveUI;

namespace CodeHub.iOS.Cells
{
    public class BranchCellView : ReactiveTableViewCell<BranchItemViewModel>
    {
        public static NSString Key = new NSString("BranchCell");

        public BranchCellView(IntPtr handle)
            : base(handle)
        {
            this.OneWayBind(ViewModel, x => x.Name, x => x.TextLabel.Text);
        }
    }
}

