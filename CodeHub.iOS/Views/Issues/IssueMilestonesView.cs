using System;
using System.Reactive.Linq;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Issues;
using System.Linq;
using CodeHub.iOS.Elements;
using MonoTouch.UIKit;
using ReactiveUI;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueMilestonesView : ViewModelCollectionView<IssueMilestonesViewModel>
    {
		public IssueMilestonesView()
		{
			Title = "Milestones";
			NoItemsText = "No Milestones";
			EnableSearch = false;
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RowHeight = 80f;
            TableView.SeparatorInset = new UIEdgeInsets(0, 0, 0, 0);

            Bind(ViewModel.WhenAnyValue(x => x.Milestones), x => {
                var e = new MilestoneElement(x);
				e.Tapped += () => {
                    if (ViewModel.SelectedMilestone != null && ViewModel.SelectedMilestone.Number == x.Number)
                        ViewModel.SelectedMilestone = null;
					else
                        ViewModel.SelectedMilestone = x;
				};
                if (ViewModel.SelectedMilestone != null && ViewModel.SelectedMilestone.Number == x.Number)
					e.Accessory = UITableViewCellAccessory.Checkmark;
				return e;
			});

            ViewModel.WhenAnyValue(x => x.SelectedMilestone).Where(x => x != null).Subscribe(x =>
			{
				if (Root.Count == 0)
					return;
				foreach (var m in Root[0].Elements.Cast<MilestoneElement>())
					m.Accessory = (x != null && m.Milestone.Number == x.Number) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
				Root.Reload(Root[0], UITableViewRowAnimation.None);
			});
        }
    }
}

