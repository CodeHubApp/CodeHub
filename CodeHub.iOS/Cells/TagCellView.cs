using System;
using ReactiveUI;
using Foundation;
using CodeHub.Core.ViewModels.Source;

namespace CodeHub.iOS.Cells
{
    public class TagCellView : ReactiveTableViewCell<TagItemViewModel>
    {
        public static NSString Key = new NSString("TagCell");

        public TagCellView(IntPtr handle)
            : base(handle)
        {
            this.WhenActivated(d => d(this.OneWayBind(ViewModel, x => x.Name, x => x.TextLabel.Text)));
        }
    }
}

