using System;
using ReactiveUI;
using UIKit;
using CodeHub.Core.ViewModels.Issues;
using Foundation;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.Cells
{
    public class MilestoneTableViewCell : ReactiveTableViewCell<IssueMilestoneItemViewModel>
    {
        public static NSString Key = new NSString("milestonecellview");
        private readonly MilestoneView _milestoneView;

        public MilestoneTableViewCell(IntPtr handle)
            : base(handle)
        {
            _milestoneView = new MilestoneView();
            _milestoneView.Frame = Frame;
            _milestoneView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            ContentView.Add(_milestoneView);

            this.OneWayBind(ViewModel, x => x.Title, x => x._milestoneView.Title);

            this.WhenAnyValue(x => x.ViewModel.IsSelected)
                .Subscribe(x => Accessory = x ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None);
            this.WhenAnyValue(x => x.ViewModel.DueDate)
                .Subscribe(x => _milestoneView.DueDate = x);
            this.WhenAnyValue(x => x.ViewModel.OpenIssues, x => x.ViewModel.ClosedIssues)
                .Subscribe(x => _milestoneView.OpenClosedIssues = new Tuple<int, int>(x.Item1, x.Item2));
        }
    }
}

