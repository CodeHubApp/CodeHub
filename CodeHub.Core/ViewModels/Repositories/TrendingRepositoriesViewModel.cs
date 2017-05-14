using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CodeHub.Core.Data;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesTrendingViewModel : ReactiveObject
    {
        private readonly Language _defaultLanguage = new Language("All Languages", null);
        private readonly ITrendingRepository _trendingRepository;

        private static readonly Tuple<string, string>[] Times = {
            Tuple.Create("Daily", "daily"),
            Tuple.Create("Weekly", "weekly"),
            Tuple.Create("Monthly", "monthly"),
        };

        private IList<Tuple<string, IList<RepositoryItemViewModel>>> _items;
        public IList<Tuple<string, IList<RepositoryItemViewModel>>> Items
        {
            get { return _items ?? new List<Tuple<string, IList<RepositoryItemViewModel>>>(); }
            private set { this.RaiseAndSetIfChanged(ref _items, value); }
        }

        private Language _selectedLanguage;
        public Language SelectedLanguage
        {
            get { return _selectedLanguage; }
            set { this.RaiseAndSetIfChanged(ref _selectedLanguage, value); }
        }

        public ReactiveCommand<Unit, Unit> LoadCommand { get; }

        public ReactiveCommand<RepositoryItemViewModel, RepositoryItemViewModel> RepositoryItemSelected { get; }

        public RepositoriesTrendingViewModel(ITrendingRepository trendingRepository = null)
        {
            _trendingRepository = trendingRepository ?? new TrendingRepository();

            RepositoryItemSelected = ReactiveCommand.Create<RepositoryItemViewModel, RepositoryItemViewModel>(x => x);

            LoadCommand = ReactiveCommand.CreateFromTask(Load);

            SelectedLanguage = _defaultLanguage;

            this.WhenAnyValue(x => x.SelectedLanguage)
                .Skip(1)
                .Select(_ => Unit.Default)
                .Do(_ => Items = null)
                .InvokeCommand(LoadCommand);
        }

        private async Task Load()
        {
            var items = new List<Tuple<string, IList<RepositoryItemViewModel>>>();

            foreach (var t in Times)
            {
                var repos = await _trendingRepository.GetTrendingRepositories(t.Item2, SelectedLanguage.Slug);
                var viewModels = repos
                    .Select(x => new RepositoryItemViewModel(x, true, true, vm => RepositoryItemSelected.ExecuteNow(vm)))
                    .ToList();
                
                items.Add(Tuple.Create(t.Item1, viewModels as IList<RepositoryItemViewModel>));
            }

            Items = items;
        }
    }
}

