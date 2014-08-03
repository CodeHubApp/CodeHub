using System;
using System.Linq;
using System.Reactive.Linq;
using CodeHub.Core.Filters;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using CodeFramework.Core.Utils;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Source
{
    public class SourceTreeViewModel : BaseViewModel, ILoadableViewModel
    {
        public IReadOnlyReactiveList<ContentModel> Content { get; private set; }

		public string Username { get; set; }

		public string Path { get; set; }

		public string Branch { get; set; }

		public bool TrueBranch { get; set; }

		public string Repository { get; set; }

        private SourceFilterModel _filter;
        public SourceFilterModel Filter
        {
            get { return _filter; }
            set { this.RaiseAndSetIfChanged(ref _filter, value); }
        }

        public IReactiveCommand<object> GoToSourceTreeCommand { get; private set; }

        public IReactiveCommand<object> GoToSubmoduleCommand { get; private set; }

        public IReactiveCommand<object> GoToSourceCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public SourceTreeViewModel(IApplicationService applicationService)
        {
            //Filter = applicationService.Account.Filters.GetFilter<SourceFilterModel>("SourceViewModel");
            var content = new ReactiveList<ContentModel>();
            Content = content.CreateDerivedCollection(x => x);

            GoToSubmoduleCommand = ReactiveCommand.Create();
            GoToSubmoduleCommand.OfType<ContentModel>().Subscribe(x =>
            {
                var nameAndSlug = x.GitUrl.Substring(x.GitUrl.IndexOf("/repos/", StringComparison.OrdinalIgnoreCase) + 7);
                var repoId = new RepositoryIdentifier(nameAndSlug.Substring(0, nameAndSlug.IndexOf("/git", StringComparison.OrdinalIgnoreCase)));
                var vm = CreateViewModel<SourceTreeViewModel>();
                vm.Username = repoId.Owner;
                vm.Repository = repoId.Name;
                vm.Branch = x.Sha;
                ShowViewModel(vm);
            });

            GoToSourceCommand = ReactiveCommand.Create();
            GoToSourceCommand.OfType<ContentModel>().Subscribe(x =>
            {
                var otherFiles = Content
                    .Where(y => string.Equals(y.Type, "file", StringComparison.OrdinalIgnoreCase))
                    .Where(y => y.Size.HasValue && y.Size.Value > 0)
                    .Select(y => new SourceViewModel.SourceItemModel 
                    {
                        Name = y.Name,
                        Path = y.Path,
                        HtmlUrl = y.HtmlUrl,
                        GitUrl = y.GitUrl
                    }).ToArray();

                var vm = CreateViewModel<SourceViewModel>();
                vm.Branch = Branch;
                vm.Username = Username;
                vm.Repository = Repository;
                vm.TrueBranch = TrueBranch;
                vm.Items = otherFiles;
                vm.CurrentItemIndex = Array.FindIndex(otherFiles, f => string.Equals(f.GitUrl, x.GitUrl, StringComparison.OrdinalIgnoreCase));
                ShowViewModel(vm);
            });

            GoToSourceTreeCommand = ReactiveCommand.Create();
            GoToSourceTreeCommand.OfType<ContentModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<SourceTreeViewModel>();
                vm.Username = Username;
                vm.Branch = Branch;
                vm.Repository = Repository;
                vm.TrueBranch = TrueBranch;
                vm.Path = x.Path;
                ShowViewModel(vm);
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
                    applicationService.Client.Users[Username].Repositories[Repository].GetContent(
                        Path ?? string.Empty, Branch ?? "master"), t as bool?));
        }
    }
}

