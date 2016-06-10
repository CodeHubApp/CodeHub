using System;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using CodeHub.Core.ViewModels;
using GitHubSharp.Models;
using CodeHub.Core.Utils;
using CodeHub.Core.Services;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Source
{
    public class SourceTreeViewModel : LoadableViewModel
    {
        private readonly IFeaturesService _featuresService;
        private readonly IApplicationService _applicationService;

        public CollectionViewModel<ContentModel> Content { get; } = new CollectionViewModel<ContentModel>();

        public string Username { get; private set; }

        public string Path { get; private set; }

        public string Branch { get; private set; }

        public bool TrueBranch { get; private set; }

        public string Repository { get; private set; }

        private bool _shouldShowPro; 
        public bool ShouldShowPro
        {
            get { return _shouldShowPro; }
            private set { this.RaiseAndSetIfChanged(ref _shouldShowPro, value); }
        }

        public ReactiveUI.IReactiveCommand<object> GoToItemCommand { get; }
            
        public SourceTreeViewModel(IApplicationService applicationService, IFeaturesService featuresService)
        {
            _applicationService = applicationService;
            _featuresService = featuresService;

            GoToItemCommand = ReactiveUI.ReactiveCommand.Create();
            GoToItemCommand.OfType<ContentModel>().Subscribe(x => {
                if (x.Type.Equals("dir", StringComparison.OrdinalIgnoreCase))
                {
                    ShowViewModel<SourceTreeViewModel>(new NavObject { Username = Username, Branch = Branch, 
                        Repository = Repository, Path = x.Path, TrueBranch = TrueBranch });
                }
                if (x.Type.Equals("file", StringComparison.OrdinalIgnoreCase))
                {
                    if (x.DownloadUrl == null)
                    {
                        var nameAndSlug = x.GitUrl.Substring(x.GitUrl.IndexOf("/repos/", StringComparison.Ordinal) + 7);
                        var indexOfGit = nameAndSlug.LastIndexOf("/git", StringComparison.Ordinal);
                        indexOfGit = indexOfGit < 0 ? 0 : indexOfGit;
                        var repoId = RepositoryIdentifier.FromFullName(nameAndSlug.Substring(0, indexOfGit));
                        if (repoId == null)
                            return;

                        var sha = x.GitUrl.Substring(x.GitUrl.LastIndexOf("/", StringComparison.Ordinal) + 1);
                        ShowViewModel<SourceTreeViewModel>(new NavObject {Username = repoId?.Owner, Repository = repoId?.Name, Branch = sha});
                    }
                    else
                    {
                        ShowViewModel<SourceViewModel>(new SourceViewModel.NavObject { 
                            Name = x.Name, Username = Username, Repository = Repository, Branch = Branch, 
                            Path = x.Path, HtmlUrl = x.HtmlUrl, GitUrl = x.GitUrl, TrueBranch = TrueBranch });
                    }
                }
            });
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            Branch = navObject.Branch ?? "master";
            Path = navObject.Path ?? "";
            TrueBranch = navObject.TrueBranch;
        }

        protected override Task Load()
        {
            if (_featuresService.IsProEnabled)
                ShouldShowPro = false;
            else
            {
                var request = _applicationService.Client.Users[Username].Repositories[Repository].Get();
                _applicationService.Client.ExecuteAsync(request)
                    .ToBackground(x => ShouldShowPro = x.Data.Private && !_featuresService.IsProEnabled);
            }
            
            return Content.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].GetContent(Path, Branch));
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

