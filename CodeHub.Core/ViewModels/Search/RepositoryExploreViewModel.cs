using System;
using System.Reactive;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Repositories;
using Octokit;
using ReactiveUI;
using Splat;

namespace CodeHub.Core.ViewModels.Search
{
    public class RepositoryExploreViewModel : ReactiveObject
    {
        private readonly IApplicationService _applicationService;

        public ReactiveCommand<Unit, Unit> SearchCommand { get; }

        public IReadOnlyReactiveList<RepositoryItemViewModel> Items { get; private set; }

        public ReactiveCommand<RepositoryItemViewModel, RepositoryItemViewModel> RepositoryItemSelected { get; }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { this.RaiseAndSetIfChanged(ref _searchText, value); }
        }

        public RepositoryExploreViewModel(
            IApplicationService applicationService = null,
            IAlertDialogService dialogService = null)
        {
            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            dialogService = dialogService ?? Locator.Current.GetService<IAlertDialogService>();

            RepositoryItemSelected = ReactiveCommand.Create<RepositoryItemViewModel, RepositoryItemViewModel>(x => x);
            var showDescription = _applicationService.Account.ShowRepositoryDescriptionInList;

            var internalItems = new ReactiveList<Repository>(resetChangeThreshold: double.MaxValue);

            Items = internalItems.CreateDerivedCollection(x =>
                new RepositoryItemViewModel(x, true, showDescription, y => RepositoryItemSelected.ExecuteNow(y)));

            var canSearch = this.WhenAnyValue(x => x.SearchText).Select(x => !string.IsNullOrEmpty(x));
            SearchCommand = ReactiveCommand.CreateFromTask(async t =>
            {
                try
                {
                    internalItems.Clear();
                    var request = new SearchRepositoriesRequest(SearchText);
                    var response = await _applicationService.GitHubClient.Search.SearchRepo(request);
                    internalItems.Reset(response.Items);
                }
                catch (Exception e)
                {
                    var msg = string.Format("Unable to search for {0}. Please try again.", SearchText);
                    throw new Exception(msg, e);
                }
            }, canSearch);

            SearchCommand
                .ThrownExceptions
                .Select(err => new UserError("Unable to complete search!", err))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();
        }
    }
}

