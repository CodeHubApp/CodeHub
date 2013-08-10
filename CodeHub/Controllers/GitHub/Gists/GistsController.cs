using System;
using CodeHub.Controllers;
using GitHubSharp.Models;
using System.Collections.Generic;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using CodeHub.Data;
using CodeFramework.Controllers;
using CodeFramework.Elements;

namespace CodeHub.GitHub.Controllers.Gists
{
    public class AccountGistsController : GistsController
    {
        private string _username;

        public AccountGistsController(string username)
        {
            if (username != null)
            {
                if (Application.Accounts.ActiveAccount.Username.Equals(_username))
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

            _username = username;
        }

        protected override object OnUpdateListModel(bool forced, int currentPage, ref int nextPage)
        {
            var data = Application.Client.API.GetGists(_username, currentPage);
            nextPage = data.Next == null ? -1 : currentPage + 1;
            return data.Data;
        }
    }

    public class StarredGistsController : GistsController
    {
        public StarredGistsController()
        {
            Title = "Starred Gists";
        }

        protected override object OnUpdateListModel(bool forced, int currentPage, ref int nextPage)
        {
            var data = Application.Client.API.GetStarredGists(currentPage);
            nextPage = data.Next == null ? -1 : currentPage + 1;
            return data.Data;
        }
    }

    public class PublicGistsController : GistsController
    {
        public PublicGistsController()
        {
            Title = "Public Gists";
        }

        protected override object OnUpdateListModel(bool forced, int currentPage, ref int nextPage)
        {
            var data = Application.Client.API.GetPublicGists(currentPage);
            nextPage = data.Next == null ? -1 : currentPage + 1;
            return data.Data;
        }
    }


    public abstract class GistsController : BaseListModelController
    {
        public GistsController()
            : base(typeof(List<GistModel>))
        {
            Title = "Gists";
        }

        protected override Element CreateElement(object obj)
        {
            var x = (GistModel)obj;
            var str = string.IsNullOrEmpty(x.Description) ? "Gist " + x.Id : x.Description;
            var sse = new NameTimeStringElement() { 
                Time = x.UpdatedAt.ToDaysAgo(), 
                String = str, 
                Lines = 4, 
                Image = CodeFramework.Images.Misc.Anonymous
            };

            sse.Name = (x.User == null) ? "Anonymous" : x.User.Login;
            sse.ImageUri = (x.User == null) ? null : new Uri(x.User.AvatarUrl);
            sse.Tapped += () => NavigationController.PushViewController(new GistInfoController(x.Id) { Model = x }, true);
            return sse;
        }
    }
}

