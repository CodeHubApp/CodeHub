using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels;
using CodeHub.Core.Filters;
using GitHubSharp.Models;
using CodeHub.Core.Utils;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Source
{
    public class SourceTreeViewModel : LoadableViewModel
    {
        private readonly FilterableCollectionViewModel<ContentModel, SourceFilterModel> _content;
        private readonly IFeaturesService _featuresService;
        private SourceFilterModel _filter;

        public FilterableCollectionViewModel<ContentModel, SourceFilterModel> Content
        {
            get { return _content; }
        }

		public string Username { get; private set; }

		public string Path { get; private set; }

		public string Branch { get; private set; }

		public bool TrueBranch { get; private set; }

		public string Repository { get; private set; }

        public SourceFilterModel Filter
        {
            get { return _filter; }
            set
            {
                _filter = value;
                RaisePropertyChanged(() => Filter);
                _content.Refresh();
            }
        }

        private bool _shouldShowPro; 
        public bool ShouldShowPro
        {
            get { return _shouldShowPro; }
            private set
            {
                _shouldShowPro = value;
                RaisePropertyChanged();
            }
        }

        public ICommand GoToSourceTreeCommand
        {
			get { return new MvxCommand<ContentModel>(x => ShowViewModel<SourceTreeViewModel>(new NavObject { Username = Username, Branch = Branch, Repository = Repository, Path = x.Path, TrueBranch = TrueBranch })); }
        }

        public ICommand GoToSubmoduleCommand
        {
            get { return new MvxCommand<ContentModel>(GoToSubmodule);}
        }

        public ICommand GoToSourceCommand
        {
			get { return new MvxCommand<ContentModel>(x => ShowViewModel<SourceViewModel>(new SourceViewModel.NavObject { Name = x.Name, Username = Username, Repository = Repository, Branch = Branch, Path = x.Path, HtmlUrl = x.HtmlUrl, GitUrl = x.GitUrl, TrueBranch = TrueBranch }));}
        }

        private void GoToSubmodule(ContentModel x)
        {
            var nameAndSlug = x.GitUrl.Substring(x.GitUrl.IndexOf("/repos/", System.StringComparison.Ordinal) + 7);
            var repoId = new RepositoryIdentifier(nameAndSlug.Substring(0, nameAndSlug.IndexOf("/git", System.StringComparison.Ordinal)));
            var sha = x.GitUrl.Substring(x.GitUrl.LastIndexOf("/", System.StringComparison.Ordinal) + 1);
            ShowViewModel<SourceTreeViewModel>(new NavObject {Username = repoId.Owner, Repository = repoId.Name, Branch = sha});
        }

        public SourceTreeViewModel(IFeaturesService featuresService)
        {
            _featuresService = featuresService;
            _content = new FilterableCollectionViewModel<ContentModel, SourceFilterModel>("SourceViewModel");
            _content.FilteringFunction = FilterModel;
            _content.Bind(x => x.Filter).Subscribe(_ => _content.Refresh());
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            Branch = navObject.Branch ?? "master";
            Path = navObject.Path ?? "";
			TrueBranch = navObject.TrueBranch;
        }

        private IEnumerable<ContentModel> FilterModel(IEnumerable<ContentModel> model)
        {
            var ret = model;
            var order = _content.Filter.OrderBy;
            if (order == SourceFilterModel.Order.Alphabetical)
                ret = model.OrderBy(x => x.Name);
            else if (order == SourceFilterModel.Order.FoldersThenFiles)
                ret = model.OrderBy(x => x.Type).ThenBy(x => x.Name);
            return _content.Filter.Ascending ? ret : ret.Reverse();
        }

        protected override Task Load(bool forceCacheInvalidation)
        {
            if (_featuresService.IsProEnabled)
                ShouldShowPro = false;
            else
                this.RequestModel(this.GetApplication().Client.Users[Username].Repositories[Repository].Get(), false, x => ShouldShowPro = x.Data.Private && !_featuresService.IsProEnabled);
            
			return Content.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].GetContent(Path, Branch), forceCacheInvalidation);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public string Branch { get; set; }
            public string Path { get; set; }

			// Whether the branch is a real branch and not a node
			public bool TrueBranch { get; set; }
        }
    }
}

