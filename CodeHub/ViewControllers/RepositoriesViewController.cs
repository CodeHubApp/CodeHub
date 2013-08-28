using MonoTouch.Dialog;
using MonoTouch.UIKit;
using GitHubSharp.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Drawing;
using CodeFramework.Controllers;
using CodeFramework.Elements;
using CodeFramework.Views;
using CodeFramework.Filters.Controllers;
using CodeHub.Filters.Models;
using CodeHub.Controllers;


namespace CodeHub.ViewControllers
{
    public class RepositoriesViewController : BaseListControllerDrivenViewController, IListView<RepositoryModel>
    {
        public string Username { get; private set; }
        public bool ShowOwner { get; set; }

		public new RepositoriesController Controller
		{
			get { return (RepositoriesController)base.Controller; }
			protected set { base.Controller = value; }
		}
    
        public RepositoriesViewController(string username, bool refresh = true)
            : base(refresh: refresh)
        {
            Username = username;
            ShowOwner = false;
            EnableFilter = true;
            Title = "Repositories".t();
            SearchPlaceholder = "Search Repositories".t();
            NoItemsText = "No Repositories".t();

            Controller = new RepositoriesController(this, username);
        }

        public void Render(ListModel<RepositoryModel> model)
        {
            RenderList(model, repo => {
                var description = Application.Account.HideRepositoryDescriptionInList ? string.Empty : repo.Description;
                var imageUrl = repo.Fork ? CodeHub.Images.GitHubRepoForkUrl : CodeHub.Images.GitHubRepoUrl;
                var sse = new RepositoryElement(repo.Name, repo.Watchers, repo.Forks, description, repo.Owner.Login, imageUrl) { ShowOwner = ShowOwner };
                sse.Tapped += () => NavigationController.PushViewController(new RepositoryInfoViewController(repo.Owner.Login, repo.Name), true);
                return sse;
            });
        }

        protected override FilterViewController CreateFilterController()
        {
            return new CodeHub.Filters.ViewControllers.RepositoriesFilterViewController(Controller);
        }
    }
}