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

        public new IssueMilestonesViewModel ViewModel
        {
            get { return (IssueMilestonesViewModel)base.ViewModel; }
            protected set { base.ViewModel = value; }
        }

        public override void ViewDidLoad()
        {
            Title = "Milestones".t();
            NoItemsText = "No Milestones".t();

            base.ViewDidLoad();

            //Add a fake 'Unassigned' guy so we can always unassigned what we've done
            ViewModel.BindCollection(x => x.Milestones, (ev) =>
            {
                var items = ViewModel.Milestones.ToList();
                var noMilestone = new MilestoneModel { Title = "No Milestone".t() };
                items.Insert(0, noMilestone);

                RenderList(items, x =>
                {
                    return new StyledStringElement(x.Title, () =>
                    {
                        if (MilestoneSelected != null)
                            MilestoneSelected(x == noMilestone ? null : x);
                    });
                }, ViewModel.Milestones.MoreItems);
            });
        }
    }
}

