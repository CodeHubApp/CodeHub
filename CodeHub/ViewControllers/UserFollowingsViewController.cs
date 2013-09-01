using System;
using CodeFramework.Controllers;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using CodeFramework.Elements;
using CodeHub.Controllers;

namespace CodeHub.ViewControllers
{
    public class UserFollowingsViewController : BaseListControllerDrivenViewController, IListView<BasicUserModel>
    {
        public UserFollowingsViewController(string user)
        {
            Title = "Following".t();
            SearchPlaceholder = "Search Following".t();
            NoItemsText = "Not Following Anyone".t();
            Controller = new UserFollowingsController(this, user);
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
}

