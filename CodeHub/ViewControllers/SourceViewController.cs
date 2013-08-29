using MonoTouch.Dialog;
using GitHubSharp.Models;
using CodeHub.Controllers;
using CodeFramework.Controllers;
using CodeFramework.Elements;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using CodeHub.Filters.Models;
using CodeHub.Controlleres;
using System;

namespace CodeHub.ViewControllers
{
    public class SourceViewController : BaseListControllerDrivenViewController, IListView<ContentModel>
    {
        private readonly string _username;
        private readonly string _slug;
        private readonly string _branch;
        private readonly string _path;

        public new SourceController Controller
        {
            get { return (SourceController)base.Controller; }
            protected set { base.Controller = value; }
        }

        public SourceViewController(string username, string slug, string branch = "master", string path = "")
        {
            _username = username;
            _slug = slug;
            _branch = branch;
            _path = path;
            EnableSearch = true;
            EnableFilter = true;
            SearchPlaceholder = "Search Files & Folders".t();
            Title = string.IsNullOrEmpty(path) ? "Source".t() : path.Substring(path.LastIndexOf('/') + 1);
            Controller = new SourceController(this, username, slug, branch, path);
        }

        public void Render(ListModel<ContentModel> model)
        {
            RenderList(model, x => {
                if (x.Type.Equals("dir", StringComparison.OrdinalIgnoreCase))
                {
                    return new StyledStringElement(x.Name, () => NavigationController.PushViewController(
                        new SourceViewController(_username, _slug, _branch, x.Path), true), Images.Folder);
                }
                else if (x.Type.Equals("file", StringComparison.OrdinalIgnoreCase))
                {
                    //If there's a size, it's a file
                    if (x.Size != null)
                    {
                        return new StyledStringElement(x.Name, () => NavigationController.PushViewController(
                            new SourceInfoViewController(_username, _slug, _branch, x.Path) { Title = x.Name }, true), Images.File);
                    }
                    //If there is no size, it's most likey a submodule
                    else
                    {
                        var nameAndSlug = x.GitUrl.Substring(x.GitUrl.IndexOf("/repos/") + 7);
                        var repoId = new CodeHub.Utils.RepositoryIdentifier(nameAndSlug.Substring(0, nameAndSlug.IndexOf("/git")));
                        var sha = x.GitUrl.Substring(x.GitUrl.LastIndexOf("/") + 1);
                        return new StyledStringElement(x.Name, () => NavigationController.PushViewController(
                            new SourceViewController(repoId.Owner, repoId.Name, sha) { Title = x.Name }, true), Images.Repo);
                    }
                }
                else
                {
                    return new StyledStringElement(x.Name) { Image = Images.File };
                }


            });
        }

        protected override CodeFramework.Filters.Controllers.FilterViewController CreateFilterController()
        {
            return new CodeHub.Filters.ViewControllers.SourceFilterViewController(Controller);
        }
    }
}

