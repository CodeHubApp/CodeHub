using System;
using ReactiveUI;
using GitHubSharp.Models;
using System.Reactive.Linq;
using MonoTouch.Foundation;

namespace CodeHub.iOS.Cells
{
    public class TeamCellView : ReactiveTableViewCell<TeamShortModel>
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

