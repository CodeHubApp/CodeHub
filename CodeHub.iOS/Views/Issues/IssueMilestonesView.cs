using CodeHub.Core.ViewModels.Issues;
using MonoTouch.UIKit;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueMilestonesView : ReactiveTableViewController<IssueMilestonesViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RowHeight = 80f;
            TableView.SeparatorInset = new UIEdgeInsets(0, 0, 0, 0);
//
//            this.BindList(ViewModel.Milestones, x => {
//                var e = new MilestoneElement(x);
//				e.Tapped += () => {
//                    if (ViewModel.SelectedMilestone != null && ViewModel.SelectedMilestone.Number == x.Number)
//                        ViewModel.SelectedMilestone = null;
//					else
//                        ViewModel.SelectedMilestone = x;
//				};
//                if (ViewModel.SelectedMilestone != null && ViewModel.SelectedMilestone.Number == x.Number)
//					e.Accessory = UITableViewCellAccessory.Checkmark;
//				return e;
//			});
//
//            ViewModel.WhenAnyValue(x => x.SelectedMilestone).Where(x => x != null).Subscribe(x =>
//			{
//				if (Root.Count == 0)
//					return;
//				foreach (var m in Root[0].Cast<MilestoneElement>())
//					m.Accessory = (x != null && m.Milestone.Number == x.Number) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
//				Root.Reload(Root[0], UITableViewRowAnimation.None);
//			});
        }
    }
}

