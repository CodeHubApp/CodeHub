using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels.Issues;
using System.Linq;
using UIKit;
using CodeFramework.iOS.Utils;
using CodeFramework.iOS.Elements;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueMilestonesView : ViewModelCollectionDrivenDialogViewController
    {
		public IssueMilestonesView()
		{
			Title = "Milestones".t();
			NoItemsText = "No Milestones".t();
			EnableSearch = false;
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RowHeight = 80f;
            TableView.SeparatorInset = new UIEdgeInsets(0, 0, 0, 0);

			var vm = (IssueMilestonesViewModel)ViewModel;
			BindCollection(vm.Milestones, x => {
                var e = new MilestoneElement(x.Number, x.Title, x.OpenIssues, x.ClosedIssues, x.DueOn);
				e.Tapped += () => {
					if (vm.SelectedMilestone != null && vm.SelectedMilestone.Number == x.Number)
						vm.SelectedMilestone = null;
					else
						vm.SelectedMilestone = x;
				};
				if (vm.SelectedMilestone != null && vm.SelectedMilestone.Number == x.Number)
					e.Accessory = UITableViewCellAccessory.Checkmark;
				return e;
			});

			vm.Bind(x => x.SelectedMilestone, x =>
			{
				if (Root.Count == 0)
					return;
				foreach (var m in Root[0].Elements.Cast<MilestoneElement>())
					m.Accessory = (x != null && m.Number == x.Number) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
				Root.Reload(Root[0], UITableViewRowAnimation.None);
			});

			var _hud = new Hud(View);
			vm.Bind(x => x.IsSaving, x =>
			{
				if (x) _hud.Show("Saving...");
				else _hud.Hide();
			});
        }
    }
}

