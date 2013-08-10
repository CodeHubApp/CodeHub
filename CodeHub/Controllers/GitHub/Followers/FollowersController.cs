using MonoTouch.Dialog;
using GitHubSharp.Models;
using ProfileController = CodeHub.GitHub.Controllers.Accounts.ProfileController;
using CodeHub.Controllers;
using CodeFramework.Controllers;
using CodeFramework.Elements;
using System.Collections.Generic;

namespace CodeHub.GitHub.Controllers.Followers
{
	public abstract class FollowersController : BaseListModelController
    {
		protected FollowersController()
            : base(typeof(List<BasicUserModel>))
		{
            Title = "Followers";
            SearchPlaceholder = "Search Followers";
            NoItemsText = "No Followers";
		}

        protected override Element CreateElement(object obj)
        {
            var s = (BasicUserModel)obj;
            StyledStringElement sse = new UserElement(s.Login, null, null, s.AvatarUrl);
            sse.Tapped += () => NavigationController.PushViewController(new ProfileController(s.Login), true);
            return sse;
        }
	}
}
