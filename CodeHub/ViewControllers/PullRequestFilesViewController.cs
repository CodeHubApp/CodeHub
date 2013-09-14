using System;
using CodeFramework.Controllers;
using GitHubSharp.Models;
using CodeHub.Controllers;
using MonoTouch.Dialog;

namespace CodeHub.ViewControllers
{
    public class PullRequestFilesViewController : BaseListControllerDrivenViewController, IListView<CommitModel.CommitFileModel>
    {
        public new PullRequestFilesController Controller
        {
            get { return (PullRequestFilesController)base.Controller; }
            protected set { base.Controller = value; }
        }

        public PullRequestFilesViewController(string username, string slug, long id)
        {
            Title = "Files";
            SearchPlaceholder = "Search Files".t();
            NoItemsText = "No Files".t();
            Controller = new PullRequestFilesController(this, username, slug, id);
        }

        
        public void Render(ListModel<CommitModel.CommitFileModel> model)
        {
            RenderList(model, x => {
                var name = x.Filename.Substring(x.Filename.LastIndexOf("/") + 1);
                var el = new StyledStringElement(name, x.Status, MonoTouch.UIKit.UITableViewCellStyle.Subtitle);
                el.Image = Images.File;
                el.Accessory = MonoTouch.UIKit.UITableViewCellAccessory.DisclosureIndicator;
                el.Tapped += () => NavigationController.PushViewController(
                    new RawContentViewController(x.RawUrl) { Title = x.Filename }, true);
                return el;
            });
        }
    }
}



