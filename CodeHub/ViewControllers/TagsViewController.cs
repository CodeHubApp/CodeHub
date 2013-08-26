using System;
using CodeFramework.Controllers;
using CodeHub.Controllers;
using System.Collections.Generic;
using MonoTouch.Dialog;
using System.Linq;
using GitHubSharp.Models;

namespace CodeHub.ViewControllers
{
    public class TagsViewController : BaseListControllerDrivenViewController, IListView<TagModel>
    {
        private readonly string _username;
        private readonly string _slug;

        public TagsViewController(string username, string slug)
        {
            _username = username;
            _slug = slug;
            Title = "Tags".t();
            SearchPlaceholder = "Search Tags".t();
            NoItemsText = "No Tags".t();
            EnableSearch = true;
            Controller = new TagsController(this, username, slug);
        }

        public void Render(ListModel<TagModel> model)
        {
            RenderList(model, x => new StyledStringElement(x.Name, () => NavigationController.PushViewController(new SourceViewController(_username, _slug, x.Commit.Sha), true)));
        }
    }
}

