using System;
using CodeFramework.Controllers;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using CodeHub.GitHub.Controllers.Gists;

namespace CodeHub.ViewControllers
{
    public class PublicGistsViewController : GistsViewController
    {
        public PublicGistsViewController()
        {
            Title = "Public Gists".t();
            Controller = new PublicGistsController(this);
        }
    }

    public class StarredGistsViewController : GistsViewController
    {
        public StarredGistsViewController()
        {
            Title = "Starred Gists".t();
            Controller = new StarredGistsController(this);
        }
    }

    public class AccountGistsViewController : GistsViewController
    {
        public AccountGistsViewController(string username)
        {
            if (username != null)
            {
                if (Application.Accounts.ActiveAccount.Username.Equals(username))
                    Title = "My Gists";
                else
                {
                    if (username.EndsWith("s"))
                        Title = username + "' Gists";
                    else
                        Title = username + "'s Gists";
                }
            }
            else
            {
                Title = "Gists";
            }

            Controller = new AccountGistsController(this, username);
        }
    }


    public abstract class GistsViewController : BaseListControllerDrivenViewController, IListView<GistModel>
    {
        protected GistsViewController()
        {
            SearchPlaceholder = "Search Gists".t();
            NoItemsText = "No Gists".t();
        }

        public void Render(ListModel<GistModel> model)
        {
            RenderList(model, x => {
                var str = string.IsNullOrEmpty(x.Description) ? "Gist " + x.Id : x.Description;
                var sse = new NameTimeStringElement() { 
                    Time = x.UpdatedAt.ToDaysAgo(), 
                    String = str, 
                    Lines = 4, 
                    Image = CodeFramework.Images.Misc.Anonymous
                };

                sse.Name = (x.User == null) ? "Anonymous" : x.User.Login;
                sse.ImageUri = (x.User == null) ? null : new Uri(x.User.AvatarUrl);
                //sse.Tapped += () => NavigationController.PushViewController(new GistInfoController(x.Id) { Model = x }, true);
                return sse;
            });
        }

    }
}

