using System;
using ReactiveUI;
using System.Reactive.Linq;
using MonoTouch.Foundation;
using CodeHub.Core.ViewModels.Organizations;

namespace CodeHub.iOS.Cells
{
    public class TeamCellView : ReactiveTableViewCell<TeamItemViewModel>
    {
        public static NSString Key = new NSString("TeamCell");

        public TeamCellView(IntPtr handle)
            : base(handle)
        {
            this.WhenAnyValue(x => x.ViewModel.Name)
                .IsNotNull().Subscribe(x => TextLabel.Text = x);
        }
    }
}

