using System;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;
using CodeHub.Core.Utilities;
using System.Reactive;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Contents;

namespace CodeHub.Core.ViewModels.Source
{
    public class SourceTreeViewModel : BaseViewModel, ILoadableViewModel, IProvidesSearchKeyword
    {
        public IReadOnlyReactiveList<SourceItemViewModel> Content { get; private set; }

		public string RepositoryOwner { get; set; }

        private string _path;
		public string Path
        {
            get { return _path; }
            set { this.RaiseAndSetIfChanged(ref _path, value); }
        }

		public string Branch { get; set; }

        private bool _trueBranch;
        public bool TrueBranch
        {
            get { return _trueBranch; }
            set { this.RaiseAndSetIfChanged(ref _trueBranch, value); }
        }

        private string _repositoryName;
		public string RepositoryName 
        {
            get { return _repositoryName; }
            set { this.RaiseAndSetIfChanged(ref _repositoryName, value); }
        }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        private bool? _pushAccess;
        public bool? PushAccess
        {
            get { return _pushAccess; }
            set { this.RaiseAndSetIfChanged(ref _pushAccess, value); }
        }

        private readonly ObservableAsPropertyHelper<bool> _canAddFile;
        public bool CanAddFile
        {
            get { return _canAddFile.Value; }
        }

        public IReactiveCommand<object> GoToSourceCommand { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public IReactiveCommand<object> GoToAddFileCommand { get; private set; }

        public SourceTreeViewModel(ISessionService applicationService)
        {
            Branch = "master";
            Path = string.Empty;

            var content = new ReactiveList<ContentModel>();
            Content = content.CreateDerivedCollection(
                x => CreateSourceItemViewModel(x),
                filter: x => x.Name.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            _canAddFile = this.WhenAnyValue(x => x.TrueBranch, y => y.PushAccess)
                .Select(x => x.Item1 && x.Item2.HasValue && x.Item2.Value)
                .ToProperty(this, x => x.CanAddFile);

            this.WhenAnyValue(x => x.Path, y => y.RepositoryName, (x, y) => new { Path = x, Repo = y })
                .Subscribe(x =>
                {
                    if (string.IsNullOrEmpty(x.Path))
                    {
                        Title = string.IsNullOrEmpty(x.Repo) ? "Source" : x.Repo;
                    }
                    else
                    {
                        var split = x.Path.TrimEnd('/').Split('/');
                        Title = split[split.Length - 1];
                    }
                });

            GoToAddFileCommand = ReactiveCommand.Create(
                this.WhenAnyValue(x => x.PushAccess, x => x.TrueBranch)
                .Select(x => x.Item1.HasValue && x.Item1.Value && x.Item2))
                .WithSubscription(_ => {
                    var vm = this.CreateViewModel<CreateFileViewModel>();
                    vm.Init(RepositoryOwner, RepositoryName, Path, Branch);
                    vm.SaveCommand.Subscribe(z => LoadCommand.ExecuteIfCan());
                    NavigateTo(vm);
                });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ => {
                if (!PushAccess.HasValue)
                {
                    applicationService.GitHubClient.Repository.Get(RepositoryOwner, RepositoryName)
                        .ToBackground(x => PushAccess = x.Permissions.Push);
                }

                var path = Path;
                if (string.Equals(path, "/", StringComparison.OrdinalIgnoreCase))
                    path = string.Empty;

                var request = applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetContent(Path, Branch);
                var data = new List<ContentModel>();
                var response = await applicationService.Client.ExecuteAsync(request);
                data.AddRange(response.Data);
                while (response.More != null)
                {
                    response = await applicationService.Client.ExecuteAsync(response.More);
                    data.AddRange(response.Data);
                }
                content.Reset(data.OrderBy(y => y.Type).ThenBy(y => y.Name));
            });
        }

        private SourceItemViewModel CreateSourceItemViewModel(ContentModel content)
        {
            return new SourceItemViewModel(content.Name, GetSourceItemType(content), x =>
            {
                switch (x.Type)
                {
                    case SourceItemType.File:
                    {
                        var vm = this.CreateViewModel<SourceViewModel>();
                        vm.Branch = Branch;
                        vm.RepositoryOwner = RepositoryOwner;
                        vm.RepositoryName = RepositoryName;
                        vm.TrueBranch = TrueBranch;
                        vm.Name = content.Name;
                        vm.HtmlUrl = content.HtmlUrl;
                        vm.Path = content.Path;
                        vm.PushAccess = PushAccess;
                        NavigateTo(vm);
                        break;
                    }
                    case SourceItemType.Directory:
                    {
                        var vm = this.CreateViewModel<SourceTreeViewModel>();
                        vm.RepositoryOwner = RepositoryOwner;
                        vm.Branch = Branch;
                        vm.RepositoryName = RepositoryName;
                        vm.TrueBranch = TrueBranch;
                        vm.Path = content.Path;
                        vm.PushAccess = PushAccess;
                        NavigateTo(vm);
                        break;
                    }
                    case SourceItemType.Submodule:
                    {
                        var nameAndSlug = content.GitUrl.Substring(content.GitUrl.IndexOf("/repos/", StringComparison.OrdinalIgnoreCase) + 7);
                        var repoId = new RepositoryIdentifier(nameAndSlug.Substring(0, nameAndSlug.IndexOf("/git", StringComparison.OrdinalIgnoreCase)));
                        var vm = this.CreateViewModel<SourceTreeViewModel>();
                        vm.RepositoryOwner = repoId.Owner;
                        vm.RepositoryName = repoId.Name;
                        vm.Branch = content.Sha;
                        NavigateTo(vm);
                        break;
                    }
                }
            });
        }

        private static SourceItemType GetSourceItemType(ContentModel content)
        {
            if (content.Type.Equals("dir", StringComparison.OrdinalIgnoreCase))
                return SourceItemType.Directory;
            if (content.Type.Equals("file", StringComparison.OrdinalIgnoreCase))
            {
                var isTree = content.GitUrl.EndsWith("trees/" + content.Sha, StringComparison.OrdinalIgnoreCase);
                if (content.Size == null || isTree)
                    return SourceItemType.Submodule;
            }

            return SourceItemType.File;
        }
    }
}

