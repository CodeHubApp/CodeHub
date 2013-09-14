using System;
using CodeFramework.Controllers;
using GitHubSharp.Models;
using CodeHub.Controllers;
using MonoTouch.Dialog;
using CodeFramework.Elements;

namespace CodeHub.ViewControllers
{
    public class IssueAssigneesViewController : BaseListControllerDrivenViewController, IListView<BasicUserModel>
    {
        public Action<BasicUserModel> SelectedUser;

		public IssueAssigneesViewController(string user, string repo)
        {
            Title = "Assignees".t();
            NoItemsText = "No Assignees".t();
            SearchPlaceholder = "Search Assignees".t();
            Controller = new RepositoryCollaboratorsController(this, user, repo);
        }

        public void Render(ListModel<BasicUserModel> model)
        {
            this.RenderList(model, x => {
                var e = new UserElement(x.Login, string.Empty, string.Empty, x.AvatarUrl);
                e.Accessory = MonoTouch.UIKit.UITableViewCellAccessory.DisclosureIndicator;
                e.Tapped += () => {
                    if (SelectedUser != null)
                        SelectedUser(x);
                };
                return e;
            });
        }
    }
}

