using GitHubSharp.Models;
using System.Collections.Generic;
using MonoTouch.Dialog;
using CodeHub.Controllers;
using CodeFramework.Elements;
using CodeFramework.Controllers;
using CodeHub.GitHub.Controllers.Source;

namespace CodeHub.GitHub.Controllers.Branches
{
    public class TagController : BaseListModelController
    {
        public string User { get; private set; }
        public string Repo { get; private set; }

        public TagController(string user, string repo)
            : base(typeof(List<TagModel>))
        {
            User = user;
            Repo = repo;
            Title = "Tags";
            SearchPlaceholder = "Search Tags";
            NoItemsText = "No Tags";
        }

        protected override object OnUpdateListModel(bool forced, int currentPage, ref int nextPage)
        {
            return Application.Client.API.GetTags(User, Repo).Data;
        }
        
        protected override Element CreateElement(object obj)
        {
            var tag = (TagModel)obj;
            return new StyledStringElement(tag.Name, () => NavigationController.PushViewController(new SourceController(User, Repo, tag.Commit.Sha), true));
        }
    }
}


