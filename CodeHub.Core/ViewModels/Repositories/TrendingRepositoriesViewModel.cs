using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CodeHub.Core.Data;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;
using CodeHub.Core.Services;
using Splat;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesTrendingViewModel : ReactiveObject
    {
        private readonly Language _defaultLanguage = new Language("All Languages", null);
        private readonly ITrendingRepository _trendingRepository;
        private readonly IAlertDialogService _dialogService;

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

        public RepositoriesTrendingViewModel(
            ITrendingRepository trendingRepository = null, 
            IAlertDialogService dialogService = null)
        {
            _trendingRepository = trendingRepository ?? new TrendingRepository();
            _dialogService = dialogService ?? Locator.Current.GetService<IAlertDialogService>();

            RepositoryItemSelected = ReactiveCommand.Create<RepositoryItemViewModel, RepositoryItemViewModel>(x => x);

            LoadCommand = ReactiveCommand.CreateFromTask(Load);
            LoadCommand.ThrownExceptions.Subscribe(LoadingError);

            SelectedLanguage = _defaultLanguage;

            this.WhenAnyValue(x => x.SelectedLanguage)
                .Skip(1)
                .Select(_ => Unit.Default)
                .Do(_ => Items = null)
                .InvokeReactiveCommand(LoadCommand);
        }

        private void LoadingError(Exception err)
        {
            var message = err.Message;
            var baseException = err.GetInnerException();
            if (baseException is System.Net.Sockets.SocketException)
            {
                message = "Unable to communicate with GitHub. " + baseException.Message;
            }

            _dialogService.Alert("Error Loading", message).ToBackground();
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

