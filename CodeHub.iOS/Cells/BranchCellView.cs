using System;
using GitHubSharp.Models;
using ReactiveUI;
using System.Reactive.Linq;
using MonoTouch.Foundation;

namespace CodeHub.iOS.Cells
{
    public class BranchCellView : ReactiveTableViewCell<BranchModel>
    {
        public static NSString Key = new NSString("BranchCell");

        public BranchCellView(IntPtr handle)
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

