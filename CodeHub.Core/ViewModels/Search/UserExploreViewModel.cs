using System;
using CodeHub.Core.Services;
using ReactiveUI;
using Octokit;
using CodeHub.Core.ViewModels.Users;
using System.Reactive;
using System.Reactive.Linq;
using Humanizer;
using Splat;

namespace CodeHub.Core.ViewModels.Search
{
    public class UserExploreViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> SearchCommand { get; }

        public IReadOnlyReactiveList<UserItemViewModel> Items { get; private set; }

        public ReactiveCommand<UserItemViewModel, UserItemViewModel> RepositoryItemSelected { get; }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { this.RaiseAndSetIfChanged(ref _searchText, value); }
        }

        public UserExploreViewModel(IApplicationService applicationService = null)
        {
            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            
            var items = new ReactiveList<Octokit.User>();
            var itemSelected = ReactiveCommand.Create<UserItemViewModel, UserItemViewModel>(x => x);

            Items = items.CreateDerivedCollection(
                x => new UserItemViewModel(x, y => itemSelected.ExecuteNow(y)));

            RepositoryItemSelected = itemSelected;

            var canSearch = this.WhenAnyValue(x => x.SearchText).Select(x => !string.IsNullOrEmpty(x));
            SearchCommand = ReactiveCommand.CreateFromTask(async t => {
                try
                {
                    items.Clear();
                    var request = new SearchUsersRequest(SearchText);
                    var response = await applicationService.GitHubClient.Search.SearchUsers(request);
                    items.Reset(response.Items);
                }
                catch (Exception e)
                {
                    var msg = string.Format("Unable to search for {0}. Please try again.", SearchText.Humanize());
                    throw new Exception(msg, e);
                }
            }, canSearch);
        }
    }
}

