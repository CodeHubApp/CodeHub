using System;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels;
using GitHubSharp.Models;
using CodeHub.Core.Utils;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Source
{
    public class SourceTreeViewModel : LoadableViewModel
    {
        private readonly IFeaturesService _featuresService;

        public CollectionViewModel<ContentModel> Content { get; }

		public string Username { get; private set; }

		public string Path { get; private set; }

		public string Branch { get; private set; }

		public bool TrueBranch { get; private set; }

		public string Repository { get; private set; }

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
            Content = new CollectionViewModel<ContentModel>();
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            Branch = navObject.Branch ?? "master";
            Path = navObject.Path ?? "";
			TrueBranch = navObject.TrueBranch;
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

