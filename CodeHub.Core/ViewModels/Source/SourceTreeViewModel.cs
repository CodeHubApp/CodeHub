using System;
using System.Linq;
using System.Reactive.Linq;
using CodeHub.Core.Filters;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using CodeFramework.Core.Utils;
using ReactiveUI;
using Xamarin.Utilities.Core.ReactiveAddons;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Source
{
    public class SourceTreeViewModel : LoadableViewModel
    {
        public ReactiveCollection<ContentModel> Content { get; private set; }

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

        public IReactiveCommand GoToSourceTreeCommand { get; private set; }

        public IReactiveCommand GoToSubmoduleCommand { get; private set; }

        public IReactiveCommand GoToSourceCommand { get; private set; }

        public SourceTreeViewModel(IApplicationService applicationService)
        {
            Filter = applicationService.Account.Filters.GetFilter<SourceFilterModel>("SourceViewModel");
            Content = new ReactiveCollection<ContentModel>();

            GoToSubmoduleCommand = new ReactiveCommand();
            GoToSubmoduleCommand.OfType<ContentModel>().Subscribe(x =>
            {
                var nameAndSlug = x.GitUrl.Substring(x.GitUrl.IndexOf("/repos/", StringComparison.OrdinalIgnoreCase) + 7);
                var repoId = new RepositoryIdentifier(nameAndSlug.Substring(0, nameAndSlug.IndexOf("/git", StringComparison.OrdinalIgnoreCase)));
                var sha = x.GitUrl.Substring(x.GitUrl.LastIndexOf("/", StringComparison.OrdinalIgnoreCase) + 1);
                var vm = CreateViewModel<SourceTreeViewModel>();
                vm.Username = repoId.Owner;
                vm.Repository = repoId.Name;
                vm.Branch = sha;
                ShowViewModel(vm);
            });

            GoToSourceCommand = new ReactiveCommand();
            GoToSourceCommand.OfType<ContentModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<SourceViewModel>();
                vm.Name = x.Name;
                vm.Username = Username;
                vm.Repository = Repository;
                vm.Branch = Branch;
                vm.Path = x.Path;
                vm.HtmlUrl = x.HtmlUrl;
                vm.GitUrl = x.GitUrl;
                vm.TrueBranch = TrueBranch;
                ShowViewModel(vm);
            });

            GoToSourceTreeCommand = new ReactiveCommand();
            GoToSourceTreeCommand.OfType<ContentModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<SourceTreeViewModel>();
                vm.Username = Username;
                vm.Branch = Branch;
                vm.Repository = Repository;
                vm.TrueBranch = TrueBranch;
                ShowViewModel(vm);
            });

            this.WhenAnyValue(x => x.Filter).Subscribe(filter =>
            {
                if (filter == null)
                {
                    Content.OrderFunc = null;
                }
                else
                {
                    Content.OrderFunc = x =>
                    {
                        switch (filter.OrderBy)
                        {
                            case SourceFilterModel.Order.FoldersThenFiles:
                                x = x.OrderBy(y => y.Type).ThenBy(y => y.Name);
                                break;
                            default:
                                x = x.OrderBy(y => y.Name);
                                break;
                        }

                        return filter.Ascending ? x : x.Reverse();
                    };
                }
            });

            LoadCommand.RegisterAsyncTask(t =>
                Content.SimpleCollectionLoad(
                    applicationService.Client.Users[Username].Repositories[Repository].GetContent(
                        Path ?? string.Empty, Branch ?? "master"), t as bool?));
        }
    }
}

