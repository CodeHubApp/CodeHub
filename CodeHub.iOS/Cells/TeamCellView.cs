using System;
using ReactiveUI;
using System.Reactive.Linq;
using MonoTouch.Foundation;
using CodeHub.Core.ViewModels.Teams;

namespace CodeHub.iOS.Cells
{
    public class TeamCellView : ReactiveTableViewCell<TeamItemViewModel>
    {
        public static NSString Key = new NSString("TeamCell");

        public TeamCellView(IntPtr handle)
            : base(handle)
        {
            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    TextLabel.Text = x.Name;
                });
        }
    }
}

