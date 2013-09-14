using System;
using CodeFramework.Controllers;
using GitHubSharp.Models;
using CodeHub.Controllers;
using MonoTouch.Dialog;

namespace CodeHub.ViewControllers
{
    public class IssueMilestonesViewController : BaseListControllerDrivenViewController, IListView<MilestoneModel>
    {
        public Action<MilestoneModel> MilestoneSelected;

        public IssueMilestonesViewController(string user, string repo)
        {
            Title = "Milestones".t();
            NoItemsText = "No Milestones".t();
            SearchPlaceholder = "Search Milestones".t();
            Controller = new IssueMilestonesController(this, user, repo);
        }

        public void Render(ListModel<MilestoneModel> model)
        {
            this.RenderList(model, x => {
                return new StyledStringElement(x.Title, () => {
                    if (MilestoneSelected != null)
                        MilestoneSelected(x);
                });
            });
        }
    }
}

