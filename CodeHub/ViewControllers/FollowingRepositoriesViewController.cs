using System;
using GitHubSharp.Models;
using System.Collections.Generic;
using CodeHub.Controllers;

namespace CodeHub.ViewControllers
{
	public class RepositoriesStarredViewController : RepositoriesViewController
    {
		public RepositoriesStarredViewController()
            : base(string.Empty)
        {
            ShowOwner = true;
            Title = "Following".t();
			Controller = new RepositoriesStarredController(this);
        }
    }
}

