using MonoTouch.Dialog;
using MonoTouch.UIKit;
using GitHubSharp.Models;
using System.Linq;
using System.Collections.Generic;
using CodeHub.Controllers;
using CodeFramework.Controllers;
using CodeFramework.Elements;
using System.Threading.Tasks;

namespace CodeHub.ViewControllers
{
    public abstract class FollowersViewController : BaseListControllerDrivenViewController, IListView<BasicUserModel>
    {
        protected FollowersViewController()
		{
            Title = "Followers".t();
            SearchPlaceholder = "Search Followers".t();
            NoItemsText = "No Followers".t();
		}

        public void Render(ListModel<BasicUserModel> model)
        {
            RenderList(model, s => {
                StyledStringElement sse = new UserElement(s.Login, string.Empty, string.Empty, s.AvatarUrl);
                sse.Tapped += () => NavigationController.PushViewController(new ProfileViewController(s.Login), true);
                return sse;
            });
        }
	}

    public class UserFollowersViewController : FollowersViewController
    {
        public UserFollowersViewController(string username)
        {
            Controller = new UserFollowersController(this, username);
        }
    }

    public class RepoFollowersViewController : FollowersViewController
    {
        public RepoFollowersViewController(string username, string slug)
        {
            Controller = new RepositoryStarredController(this, username, slug);
        }
    }
}

