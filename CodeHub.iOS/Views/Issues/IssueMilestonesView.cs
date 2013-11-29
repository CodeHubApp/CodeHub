using System;
using System.Linq;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels.Issues;
using GitHubSharp.Models;
using MonoTouch.Dialog;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueMilestonesView : ViewModelCollectionDrivenViewController
    {
        public Action<MilestoneModel> MilestoneSelected;

        public override void ViewDidLoad()
        {
            Title = "Milestones".t();
            NoItemsText = "No Milestones".t();

            base.ViewDidLoad();

			var vm = (IssueMilestonesViewModel)ViewModel;

            //Add a fake 'Unassigned' guy so we can always unassigned what we've done
			vm.BindCollection(x => x.Milestones, (ev) =>
            {
				var items = vm.Milestones.ToList();
                var noMilestone = new MilestoneModel { Title = "No Milestone".t() };
                items.Insert(0, noMilestone);

                RenderList(items, x =>
                {
                    return new StyledStringElement(x.Title, () =>
                    {
                        if (MilestoneSelected != null)
                            MilestoneSelected(x == noMilestone ? null : x);
                    });
				}, vm.Milestones.MoreItems);
            });
        }
    }
}

