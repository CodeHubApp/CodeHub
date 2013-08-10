using CodeHub.Controllers;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using System.Collections.Generic;
using System.Linq;
using CodeFramework.Controllers;
using CodeFramework.Elements;

namespace CodeHub.GitHub.Controllers.Repositories
{
    public class RepositoryController : BaseListModelController
    {
        public string Username { get; private set; }
        public bool ShowOwner { get; set; }

        public RepositoryController(string username)
            : base(typeof(List<RepositoryModel>))
        {
            Username = username;
            ShowOwner = false;
            Title = "Repositories";
            SearchPlaceholder = "Search Repositories";
            NoItemsText = "No Repositories";
        }

        protected override object OnUpdateListModel(bool forced, int currentPage, ref int nextPage)
        {
            var a = Application.Client.API.ListRepositories(Username);
            nextPage = a.Next == null ? -1 : currentPage + 1;
            return a.Data.OrderBy(x => x.Name).ToList();
        }
        
        protected override Element CreateElement(object obj)
        {
            var repo = (RepositoryModel)obj;
            var sse = new RepositoryElement(repo.Name, repo.Watchers, repo.Forks, repo.Description, repo.Owner.Login, new System.Uri(string.Empty)) { ShowOwner = ShowOwner };
            sse.Tapped += () => NavigationController.PushViewController(new RepositoryInfoController(repo.Owner.Login, repo.Name), true);
            return sse;
        }
    }
}
