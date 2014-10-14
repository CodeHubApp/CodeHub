using System;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Generic;
using CodeHub.Core.Services;
using ReactiveUI;

using Xamarin.Utilities.Core.Services;
using Xamarin.Utilities.Core.ViewModels;
using CodeHub.Core.Models;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesTrendingViewModel : BaseViewModel, ILoadableViewModel
    {
        private const string TrendingUrl = "http://trending.codehub-app.com/trending";
        private readonly IApplicationService _applicationService;
        private readonly IJsonHttpClientService _jsonHttpClient;
        private readonly TimeModel[] _times = 
        {
            new TimeModel { Name = "Daily", Slug = "daily" },
            new TimeModel { Name = "Weekly", Slug = "weekly" },
            new TimeModel { Name = "Monthly", Slug = "monthly" }
        };

        private IReadOnlyList<GroupedCollection<RepositoryItemViewModel>> _repositories;
        public IReadOnlyList<GroupedCollection<RepositoryItemViewModel>> Repositories 
        {
            get { return _repositories; }
            set { this.RaiseAndSetIfChanged(ref _repositories, value); }
        }

        private LanguageItemViewModel _selectedLanguage;
        public LanguageItemViewModel SelectedLanguage
        {
            get { return _selectedLanguage; }
            set { this.RaiseAndSetIfChanged(ref _selectedLanguage, value); }
        }

        public bool ShowRepositoryDescription
        {
            get { return _applicationService.Account.ShowRepositoryDescriptionInList; }
        }

        public IReactiveCommand GoToLanguages { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public RepositoriesTrendingViewModel(IApplicationService applicationService, 
            IJsonHttpClientService jsonHttpClient, INetworkActivityService networkActivityService)
        {
            _applicationService = applicationService;
            _jsonHttpClient = jsonHttpClient;

            Title = "Trending";

            var defaultLanguage = LanguagesViewModel.DefaultLanguage;
            SelectedLanguage = new LanguageItemViewModel(defaultLanguage.Name, defaultLanguage.Slug);

            GoToLanguages = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = CreateViewModel<LanguagesViewModel>();
                vm.SelectedLanguage = SelectedLanguage;
                vm.WhenAnyValue(x => x.SelectedLanguage).Skip(1).Subscribe(x => 
                {
                    SelectedLanguage = x;
                    vm.DismissCommand.ExecuteIfCan();
                });
                ShowViewModel(vm);
            });

            var gotoRepository = new Action<RepositoryItemViewModel>(x =>
            {
                var vm = CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = x.Owner;
                vm.RepositoryName = x.Name;
                ShowViewModel(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ =>
            {
                var requests = _times.Select(t =>
                {
                    var query = "?since=" + t.Slug;
                    if (SelectedLanguage != null && SelectedLanguage.Slug != null)
                        query += string.Format("&language={0}", SelectedLanguage.Slug);
                    return new { Time = t, Query = _jsonHttpClient.Get<List<TrendingRepositoryModel>>(TrendingUrl + query) };
                }).ToArray();

                await Task.WhenAll(requests.Select(x => x.Query));

                Repositories = requests.Select(r =>
                {
                    var transformedRepos = r.Query.Result.Select(x => 
                        new RepositoryItemViewModel(x.Name, x.Owner, x.AvatarUrl, x.Description, x.Stars, x.Forks, true, gotoRepository));
                    return new GroupedCollection<RepositoryItemViewModel>(r.Time.Name, new ReactiveList<RepositoryItemViewModel>(transformedRepos));
                }).ToList();
            });

            LoadCommand.TriggerNetworkActivity(networkActivityService);
            this.WhenAnyValue(x => x.SelectedLanguage).Subscribe(_ => LoadCommand.ExecuteIfCan());
        }

        private class TimeModel
        {
            public string Name { get; set; }
            public string Slug { get; set; }
        }
    }
}

