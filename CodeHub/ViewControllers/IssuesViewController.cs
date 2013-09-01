using CodeHub.Controllers;
using MonoTouch.UIKit;
using CodeFramework.Views;

namespace CodeHub.ViewControllers
{
    public class IssuesViewController : BaseIssuesViewController
    {
        public new IssuesController Controller
        {
            get { return (IssuesController)base.Controller; }
            protected set { base.Controller = value; }
        }

        public IssuesViewController(string user, string slug)
        {
            Controller = new IssuesController(this, user, slug);

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(NavigationButton.Create(CodeFramework.Images.Buttons.Add, () => {
//                var b = new IssueEditViewController {
//                    Username = Controller.User,
//                    RepoSlug = Controller.Slug,
//                    Success = (issue) => Controller.CreateIssue(issue)
//                };
//                NavigationController.PushViewController(b, true);
            }));
        }
    }
}

