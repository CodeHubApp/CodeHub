using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels.Issues;
using MonoTouch.Dialog;
using GitHubSharp.Models;
using System.Linq;
using MonoTouch.UIKit;
using CodeFramework.iOS.Utils;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueMilestonesView : ViewModelCollectionDrivenViewController
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

			var vm = (IssueMilestonesViewModel)ViewModel;
			BindCollection(vm.Milestones, x => {
				var e = new MilestoneElement(x);
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
					m.Accessory = (x != null && m.Milestone.Number == x.Number) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
				Root.Reload(Root[0], UITableViewRowAnimation.None);
			});

			var _hud = new Hud(View);
			vm.Bind(x => x.IsSaving, x =>
			{
				if (x) _hud.Show("Saving...");
				else _hud.Hide();
			});
        }

		private class MilestoneElement : StyledStringElement
		{
			public MilestoneModel Milestone { get; private set; }
			public MilestoneElement(MilestoneModel m) 
				: base(m.Title)
			{
				Milestone = m;
			}
		}
    }
}

