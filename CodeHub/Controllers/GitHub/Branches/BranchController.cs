using CodeHub.GitHub.Controllers.Source;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using System.Collections.Generic;
using System.Linq;
using CodeHub.Controllers;
using CodeFramework.Controllers;
using CodeFramework.Elements;

namespace CodeHub.GitHub.Controllers.Branches
{
    public class BranchController : BaseListModelController
	{
        public string Username { get; private set; }
        public string Slug { get; private set; }

		public BranchController(string username, string slug) 
            : base(typeof(List<BranchModel>))
		{
            Username = username;
            Slug = slug;
            Title = "Branches";
            SearchPlaceholder = "Search Branches";
            NoItemsText = "No Branches";
		}

        protected override Element CreateElement(object obj)
        {
            var branch = (BranchModel)obj;
            return new StyledStringElement(branch.Name, () => NavigationController.PushViewController(new SourceController(Username, Slug, branch.Name), true));
        }

        protected override object OnUpdateListModel(bool forced, int currentPage, ref int nextPage)
        {
            var d = Application.Client.API.GetBranches(Username, Slug);
            return d.Data.OrderByDescending(x => x.Name).ToList();
        }
	}
}


