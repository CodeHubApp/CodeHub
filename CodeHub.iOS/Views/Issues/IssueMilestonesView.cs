using System;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Issues;
using System.Linq;
using UIKit;
using CodeHub.iOS.Utilities;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueMilestonesView : ViewModelCollectionDrivenDialogViewController
    {
        public IssueMilestonesView()
        {
            Title = "Milestones";
            EnableSearch = false;

            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Milestone.ToEmptyListImage(), "There are no milestones."));
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

            vm.Bind(x => x.SelectedMilestone).Subscribe(x =>
            {
                if (Root.Count == 0)
                    return;
                foreach (var m in Root[0].Elements.Cast<MilestoneElement>())
                    m.Accessory = (x != null && m.Number == x.Number) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
            });

            vm.Bind(x => x.IsSaving).SubscribeStatus("Saving...");
        }
    }
}

