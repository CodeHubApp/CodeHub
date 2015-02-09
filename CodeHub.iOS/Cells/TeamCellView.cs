using System;
using ReactiveUI;
using Foundation;
using CodeHub.Core.ViewModels.Organizations;

namespace CodeHub.iOS.Cells
{
    public class TeamCellView : ReactiveTableViewCell<TeamItemViewModel>
    {
        public static NSString Key = new NSString("TeamCell");

        public TeamCellView(IntPtr handle)
            : base(handle)
        {
            this.OneWayBind(ViewModel, x => x.Name, x => x.TextLabel.Text);
        }
    }
}

