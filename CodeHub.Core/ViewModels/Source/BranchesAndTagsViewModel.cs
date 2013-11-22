using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using System.Linq;

namespace CodeHub.Core.ViewModels.Source
{
	public class BranchesAndTagsViewModel : LoadableViewModel
	{
		private readonly CollectionViewModel<ViewObject> _items = new CollectionViewModel<ViewObject>();

		public string Username
		{
			get;
			private set;
		}

		public string Repository
		{
			get;
			private set;
		}

		public bool IsBranchesShowing { get; private set; }

		public CollectionViewModel<ViewObject> Items
		{
			get { return _items; }
		}

		public ICommand GoToSourceCommand
		{
			get { return new MvxCommand<ViewObject>(GoToSource); }
		}

		public ICommand ShowBranchesCommand
		{
			get
			{
				return new MvxCommand(() =>
				{
					IsBranchesShowing = true;
					LoadCommand.Execute(false);
				}, () => !IsBranchesShowing);
			}
		}

		public ICommand ShowTagsCommand
		{
			get
			{
				return new MvxCommand(() =>
				{
					IsBranchesShowing = false;
					LoadCommand.Execute(false);
				}, () => IsBranchesShowing);
			}
		}

		private void GoToSource(ViewObject obj)
		{
			if (obj.Object is BranchModel)
			{
				var x = obj.Object as BranchModel;
				ShowViewModel<SourceTreeViewModel>(new SourceTreeViewModel.NavObject { Username = Username, Repository = Repository, Branch = x.Name });
			}
			else if (obj.Object is TagModel)
			{
				var x = obj.Object as TagModel;
				ShowViewModel<SourceTreeViewModel>(new SourceTreeViewModel.NavObject { Username = Username, Repository = Repository, Branch = x.Commit.Sha });
			}
		}

		public void Init(NavObject navObject)
		{
			Username = navObject.Username;
			Repository = navObject.Repository;
			IsBranchesShowing = navObject.IsShowingBranches;
		}

		protected override Task Load(bool forceDataRefresh)
		{
			if (IsBranchesShowing)
			{
				var request = Application.Client.Users[Username].Repositories[Repository].GetBranches();
				return Task.Run(() => this.RequestModel(request, forceDataRefresh, response =>
				{
					Items.Items.Reset(response.Data.Select(x => new ViewObject { Name = x.Name, Object = x }));
					this.CreateMore(response, m => Items.MoreItems = m, d => Items.Items.AddRange(d.Select(x => new ViewObject { Name = x.Name, Object = x })));
				}));
			}
			else
			{
				var request = Application.Client.Users[Username].Repositories[Repository].GetTags();
				return Task.Run(() => this.RequestModel(request, forceDataRefresh, response => {
					Items.Items.Reset(response.Data.Select(x => new ViewObject { Name = x.Name, Object = x }));
					this.CreateMore(response, m => Items.MoreItems = m, d => Items.Items.AddRange(d.Select(x => new ViewObject { Name = x.Name, Object = x })));
				}));
			}
		}

		public class ViewObject
		{
			public string Name { get; set; }
			public object Object { get; set; }
		}

		public class NavObject
		{
			public string Username { get; set; }
			public string Repository { get; set; }
			public bool IsShowingBranches { get; set; }

			public NavObject()
			{
				IsShowingBranches = true;
			}
		}
	}
}

