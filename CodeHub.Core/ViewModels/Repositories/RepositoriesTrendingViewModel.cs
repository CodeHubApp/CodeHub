using System;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Generic;
using CodeHub.Core.Services;
using ReactiveUI;
using System.Threading.Tasks;
using CodeHub.Core.Data;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesTrendingViewModel : BaseViewModel, ILoadableViewModel
    {
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

        public bool ShowRepositoryDescription { get; private set; }

        public IReactiveCommand GoToLanguages { get; private set; }

        public IReactiveCommand<Unit> LoadCommand { get; private set; }

        public RepositoriesTrendingViewModel(ISessionService applicationService, ITrendingRepository trendingRepository)
        {
            ShowRepositoryDescription = applicationService.Account.ShowRepositoryDescriptionInList;

            Title = "Trending";

            var defaultLanguage = LanguageRepository.DefaultLanguage;
            SelectedLanguage = new LanguageItemViewModel(defaultLanguage.Name, defaultLanguage.Slug);

            GoToLanguages = ReactiveCommand.Create().WithSubscription(_ =>
            {
                var vm = this.CreateViewModel<LanguagesViewModel>();
                vm.SelectedLanguage = SelectedLanguage;
                vm.WhenAnyValue(x => x.SelectedLanguage).Skip(1)
                    .Subscribe(x => SelectedLanguage = x);
                NavigateTo(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async _ => {

                var requests = _times.Select(t =>
                {
                    var language = (SelectedLanguage != null && SelectedLanguage.Slug != null) ? SelectedLanguage.Slug : null;
                    return new { Time = t, Query = trendingRepository.GetTrendingRepositories(t.Slug, language) };
                }).ToArray();

                await Task.WhenAll(requests.Select(x => x.Query));

                Repositories = requests.Select(r =>
                {
                    var transformedRepos = r.Query.Result.Select(x => new RepositoryItemViewModel(x, true, GoToRepository));
                    return new GroupedCollection<RepositoryItemViewModel>(r.Time.Name, new ReactiveList<RepositoryItemViewModel>(transformedRepos));
                }).ToList();
            });

            this.WhenAnyValue(x => x.SelectedLanguage).Skip(1).Subscribe(_ => 
            {
                Repositories = null;
                LoadCommand.ExecuteIfCan();
            });
        }

        private void GoToRepository(RepositoryItemViewModel viewModel)
        {
            var vm = this.CreateViewModel<RepositoryViewModel>();
            vm.Init(viewModel.Owner, viewModel.Name, viewModel.Repository);
            NavigateTo(vm);
        }

        private class TimeModel
        {
            public string Name { get; set; }
            public string Slug { get; set; }
        }
    }
}

