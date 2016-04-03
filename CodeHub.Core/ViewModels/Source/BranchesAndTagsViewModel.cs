using System;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels;
using GitHubSharp.Models;
using System.Linq;

namespace CodeHub.Core.ViewModels.Source
{
    public class BranchesAndTagsViewModel : LoadableViewModel
    {
        private int _selectedFilter;
        public int SelectedFilter
        {
            get { return _selectedFilter; }
            set { this.RaiseAndSetIfChanged(ref _selectedFilter, value); }
        }

        public string Username { get; private set; }

        public string Repository { get; private set; }

        public CollectionViewModel<ViewObject> Items { get; }

        public IMvxCommand GoToSourceCommand
        {
            get { return new MvxCommand<ViewObject>(GoToSource); }
        }

        private void GoToSource(ViewObject obj)
        {
            if (obj.Object is BranchModel)
            {
                var x = obj.Object as BranchModel;
                ShowViewModel<SourceTreeViewModel>(new SourceTreeViewModel.NavObject { Username = Username, Repository = Repository, Branch = x.Name, TrueBranch = true });
            }
            else if (obj.Object is TagModel)
            {
                var x = obj.Object as TagModel;
                ShowViewModel<SourceTreeViewModel>(new SourceTreeViewModel.NavObject { Username = Username, Repository = Repository, Branch = x.Commit.Sha });
            }
        }

        public BranchesAndTagsViewModel()
        {
            Items = new CollectionViewModel<ViewObject>();
            this.Bind(x => x.SelectedFilter).Subscribe(x => LoadCommand.Execute(false));
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            _selectedFilter = navObject.IsShowingBranches ? 0 : 1;
        }

        protected override Task Load()
        {
            if (SelectedFilter == 0)
            {
                var request = this.GetApplication().Client.Users[Username].Repositories[Repository].GetBranches();
                return this.RequestModel(request, response =>
                {
                    this.CreateMore(response, m => Items.MoreItems = m, d => Items.Items.AddRange(d.Where(x => x != null).Select(x => new ViewObject { Name = x.Name, Object = x })));
                    Items.Items.Reset(response.Data.Where(x => x != null).Select(x => new ViewObject { Name = x.Name, Object = x }));
                });
            }
            else
            {
                var request = this.GetApplication().Client.Users[Username].Repositories[Repository].GetTags();
                return this.RequestModel(request, response => 
                {
                    this.CreateMore(response, m => Items.MoreItems = m, d => Items.Items.AddRange(d.Where(x => x != null).Select(x => new ViewObject { Name = x.Name, Object = x })));
                    Items.Items.Reset(response.Data.Where(x => x != null).Select(x => new ViewObject { Name = x.Name, Object = x }));
                });
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

