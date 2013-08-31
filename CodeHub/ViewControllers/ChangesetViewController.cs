using CodeFramework.Controllers;
using CodeHub.Controllers;
using MonoTouch.Dialog;
using GitHubSharp.Models;
using System;

namespace CodeHub.ViewControllers
{
    public class ChangesetViewController : BaseListControllerDrivenViewController, IListView<CommitModel>
    {
        private readonly string _user;
        private readonly string _slug;

        public ChangesetViewController(string user, string slug)
        {
            _user = user;
            _slug = slug;
            Title = "Changes".t();
            Root.UnevenRows = true;
            EnableSearch = false;
            Controller = new ChangesetController(this, user, slug);
        }

        public void Render(ListModel<CommitModel> model)
        {
            RenderList(model, x => {
                var desc = (x.Commit.Message ?? "").Replace("\n", " ").Trim();
                string login;
                var date = DateTime.MinValue;

                if (x.Committer != null)
                    login = x.Committer.Login;
                else if (x.Author != null)
                    login = x.Author.Login;
                else if (x.Commit.Committer != null)
                    login = x.Commit.Committer.Name;
                else
                    login = "Unknown";

                if (x.Commit.Committer != null)
                    date = x.Commit.Committer.Date;

                var el = new NameTimeStringElement { Name = login, Time = date.ToDaysAgo(), String = desc, Lines = 4 };
                el.Tapped += () => NavigationController.PushViewController(new ChangesetInfoViewController(_user, _slug, x.Sha), true);
                return el;
            });
        }
    }
}

