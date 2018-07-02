using System;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Issues;
using System.Linq;
using UIKit;
using CodeHub.iOS.DialogElements;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reactive;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueMilestonesView : DialogViewController
    {
        public IssueMilestonesViewModel ViewModel { get; }

        public IssueMilestonesView()
        {
            Title = "Milestones";
            EnableSearch = false;

            //EmptyView = new Lazy<UIView>(() =>
                //new EmptyListView(Octicon.Milestone.ToEmptyListImage(), "There are no milestones."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.RowHeight = 80f;
            TableView.SeparatorInset = new UIEdgeInsets(0, 0, 0, 0);

            ViewModel
                .Milestones.Changed
                .Select(_ => Unit.Default)
                .StartWith(Unit.Default)
                .Subscribe(_ =>
                {
                    var section = new Section();
                    section.AddAll(ViewModel.Milestones.Select(CreateElement));
                    Root.Reset(section);
                });

            ViewModel.WhenAnyValue(x => x.SelectedMilestone).Subscribe(x =>
            {
                if (Root.Count == 0)
                    return;
                foreach (var m in Root[0].Elements.Cast<MilestoneElement>())
                    m.Accessory = (x != null && m.Number == x.Number) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
            });

            //viewModel.Bind(x => x.IsSaving).SubscribeStatus("Saving...");
        }

        public MilestoneElement CreateElement(Octokit.Milestone milestone)
        {
            var e = new MilestoneElement(
                milestone.Number, milestone.Title, milestone.OpenIssues, milestone.ClosedIssues, milestone.DueOn);

            e.Tapped += () => {
                if (ViewModel.SelectedMilestone != null && ViewModel.SelectedMilestone.Number == milestone.Number)
                    ViewModel.SelectedMilestone = null;
                else
                    ViewModel.SelectedMilestone = milestone;
            };

            if (ViewModel.SelectedMilestone != null && ViewModel.SelectedMilestone.Number == milestone.Number)
                e.Accessory = UITableViewCellAccessory.Checkmark;
            
            return e;    
        }
    }
}

