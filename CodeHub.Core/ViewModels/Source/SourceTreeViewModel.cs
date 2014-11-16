using System;
using System.Linq;
using System.Reactive.Linq;
using CodeHub.Core.Filters;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;
using CodeHub.Core.Utilities;
using Xamarin.Utilities.Core;

namespace CodeHub.Core.ViewModels.Source
{
    public class SourceTreeViewModel : BaseViewModel, ILoadableViewModel, IProvidesSearchKeyword
    {
        public IReadOnlyReactiveList<SourceItemViewModel> Content { get; private set; }

		public string RepositoryOwner { get; set; }

		public string Path { get; set; }

		public string Branch { get; set; }

		public bool TrueBranch { get; set; }

		public string RepositoryName { get; set; }

        private SourceFilterModel _filter;
        public SourceFilterModel Filter
        {
            get { return _filter; }
            set { this.RaiseAndSetIfChanged(ref _filter, value); }
        }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public IReactiveCommand<object> GoToSourceCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public SourceTreeViewModel(IApplicationService applicationService)
        {
            //Filter = applicationService.Account.Filters.GetFilter<SourceFilterModel>("SourceViewModel");
            var content = new ReactiveList<ContentModel>();
            Content = content.CreateDerivedCollection(
                x => CreateSourceItemViewModel(x),
                filter: x => x.Name.ContainsKeyword(SearchKeyword),
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            this.WhenActivated(d =>
            {
                if (string.IsNullOrEmpty(Path))
                    Title = RepositoryName;
                else
                {
                    var path = Path.TrimEnd('/');
                    Title = path.Substring(path.LastIndexOf('/') + 1);
                } 
            });

            this.WhenAnyValue(x => x.Filter).Subscribe(filter =>
            {
//                if (filter == null)
//                {
//                    Content.OrderFunc = null;
//                }
//                else
//                {
//                    Content.OrderFunc = x =>
//                    {
//                        switch (filter.OrderBy)
//                        {
//                            case SourceFilterModel.Order.FoldersThenFiles:
//                                x = x.OrderBy(y => y.Type).ThenBy(y => y.Name);
//                                break;
//                            default:
//                                x = x.OrderBy(y => y.Name);
//                                break;
//                        }
//
//                        return filter.Ascending ? x : x.Reverse();
//                    };
//                }
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                content.SimpleCollectionLoad(
                    applicationService.Client.Users[RepositoryOwner].Repositories[RepositoryName].GetContent(
                        Path ?? string.Empty, Branch ?? "master"), t as bool?));
        }

        private SourceItemViewModel CreateSourceItemViewModel(ContentModel content)
        {
            return new SourceItemViewModel(content.Name, GetSourceItemType(content), x =>
            {
                switch (x.Type)
                {
                    case SourceItemType.File:
                    {
                        var vm = CreateViewModel<SourceViewModel>();
                        vm.Branch = Branch;
                        vm.RepositoryOwner = RepositoryOwner;
                        vm.RepositoryName = RepositoryName;
                        vm.TrueBranch = TrueBranch;
                        vm.Name = content.Name;
                        vm.HtmlUrl = content.HtmlUrl;
                        vm.Path = content.Path;
                        vm.GitUrl = content.GitUrl;
                        ShowViewModel(vm);
                        break;
                    }
                    case SourceItemType.Directory:
                    {
                        var vm = CreateViewModel<SourceTreeViewModel>();
                        vm.RepositoryOwner = RepositoryOwner;
                        vm.Branch = Branch;
                        vm.RepositoryName = RepositoryName;
                        vm.TrueBranch = TrueBranch;
                        vm.Path = content.Path;
                        ShowViewModel(vm);
                        break;
                    }
                    case SourceItemType.Submodule:
                    {
                        var nameAndSlug = content.GitUrl.Substring(content.GitUrl.IndexOf("/repos/", StringComparison.OrdinalIgnoreCase) + 7);
                        var repoId = new RepositoryIdentifier(nameAndSlug.Substring(0, nameAndSlug.IndexOf("/git", StringComparison.OrdinalIgnoreCase)));
                        var vm = CreateViewModel<SourceTreeViewModel>();
                        vm.RepositoryOwner = repoId.Owner;
                        vm.RepositoryName = repoId.Name;
                        vm.Branch = content.Sha;
                        ShowViewModel(vm);
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

