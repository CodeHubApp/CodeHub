using System;
using CodeFramework.Controllers;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using CodeHub.Controllers;

namespace CodeHub.ViewControllers
{
	public class NotificationsViewController : BaseListControllerDrivenViewController, IListView<NotificationModel>
    {
        public new NotificationsController Controller
        {
            get { return (NotificationsController)base.Controller; }
            private set { base.Controller = value; }
        }

		public NotificationsViewController()
        {
            SearchPlaceholder = "Search Notifications".t();
            NoItemsText = "No Notifications".t();
            Title = "Notifications".t();
            Controller = new NotificationsController(this);
        }

		public void Render(ListModel<NotificationModel> model)
        {
            RenderList(model, x => {
                var el = new StyledStringElement(x.Subject.Title, x.UpdatedAt.ToDaysAgo(), MonoTouch.UIKit.UITableViewCellStyle.Subtitle);
                el.Accessory = MonoTouch.UIKit.UITableViewCellAccessory.DisclosureIndicator;

                var subject = x.Subject.Type.ToLower();
                if (subject.Equals("issue"))
                {
                    el.Image = Images.Notifications.Issue;
                    el.Tapped += () => {
                        this.DoWorkNoHud(() => Controller.Read(x));
                        var node = x.Subject.Url.Substring(x.Subject.Url.LastIndexOf('/') + 1);
                        NavigationController.PushViewController(new IssueViewController(x.Repository.Owner.Login, x.Repository.Name, long.Parse(node)), true);
                    };
                }
                else if (subject.Equals("pullrequest"))
                {
                    el.Image = Images.Notifications.PullRequest;
                    el.Tapped += () => {
                        this.DoWorkNoHud(() => Controller.Read(x));
                        var node = x.Subject.Url.Substring(x.Subject.Url.LastIndexOf('/') + 1);
                        NavigationController.PushViewController(new PullRequestViewController(x.Repository.Owner.Login, x.Repository.Name, long.Parse(node)), true);
                    };
                }
                else if (subject.Equals("commit"))
                {
                    el.Image = Images.Notifications.Commit;
                    el.Tapped += () => {
                        this.DoWorkNoHud(() => Controller.Read(x));
                        var node = x.Subject.Url.Substring(x.Subject.Url.LastIndexOf('/') + 1);
                        NavigationController.PushViewController(new ChangesetInfoViewController(x.Repository.Owner.Login, x.Repository.Name, node), true);
                    };
                }

                return el;
            });
        }

    }
}

