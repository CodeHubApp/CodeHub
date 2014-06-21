using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CodeHub.Core.Services;
using ReactiveUI;
using Xamarin.Utilities.Core.ReactiveAddons;
using Xamarin.Utilities.Core.Services;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesTrendingViewModel : LoadableViewModel
    {
        private const string LanguagesUrl = "http://codehub-trending.herokuapp.com/languages";
        private const string TrendingUrl = "http://codehub-trending.herokuapp.com/trending";
        private readonly IApplicationService _applicationService;
        private readonly IJsonHttpClientService _jsonHttpClient;
        private readonly TimeModel[] _times = 
        {
            new TimeModel { Name = "Daily", Slug = "daily" },
            new TimeModel { Name = "Weekly", Slug = "weekly" },
            new TimeModel { Name = "Monthly", Slug = "monthly" }
        };
        private readonly LanguageModel _defaultLanguage = new LanguageModel { Name = "All Languages", Slug = null };

        public TimeModel[] Times
        {
            get { return _times; }
        }

        public ReactiveCollection<RepositoryModel> Repositories { get; private set; }

        public ReactiveList<LanguageModel> Languages { get; private set; }

        private LanguageModel _selectedLanguage;
        public LanguageModel SelectedLanguage
        {
            get { return _selectedLanguage; }
            set { this.RaiseAndSetIfChanged(ref _selectedLanguage, value); }
        }

        private TimeModel _selectedTime;
        public TimeModel SelectedTime
        {
            get { return _selectedTime; }
            set { this.RaiseAndSetIfChanged(ref _selectedTime, value); }
        }

        public bool ShowRepositoryDescription
        {
            get { return _applicationService.Account.ShowRepositoryDescriptionInList; }
        }

        public IReactiveCommand GoToRepositoryCommand { get; private set; }

        public RepositoriesTrendingViewModel(IApplicationService applicationService, IJsonHttpClientService jsonHttpClient)
        {
            _applicationService = applicationService;
            _jsonHttpClient = jsonHttpClient;

            Languages = new ReactiveList<LanguageModel>();
            Repositories = new ReactiveCollection<RepositoryModel>();

            GoToRepositoryCommand = new ReactiveCommand();
            GoToRepositoryCommand.OfType<RepositoryModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = x.Owner;
                vm.RepositoryName = x.Name;
                ShowViewModel(vm);
            });

            SelectedTime = Times[0];
            SelectedLanguage = _defaultLanguage;
            GetLanguages().FireAndForget();

            this.WhenAnyValue(x => x.SelectedTime, x => x.SelectedLanguage, (x, y) => Unit.Default)
                .Skip(1).Subscribe(_ => LoadCommand.ExecuteIfCan());

            LoadCommand.RegisterAsyncTask(async t =>
            {
                var query = "?";
                if (SelectedLanguage != null && SelectedLanguage.Slug != null)
                    query += string.Format("language={0}&", SelectedLanguage.Slug);
                if (SelectedTime != null && SelectedTime.Slug != null)
                    query += string.Format("since={0}", SelectedTime.Slug);

                var repos = await _jsonHttpClient.Get<List<RepositoryModel>>(TrendingUrl + query);
                Repositories.Reset(repos);
            });
        }

        private async Task GetLanguages()
        {
            var languages = await _jsonHttpClient.Get<List<LanguageModel>>(LanguagesUrl);
            languages.Insert(0, _defaultLanguage);
            Languages.Reset(languages);
        }

        public class LanguageModel
        {
            public string Name { get; set; }
            public string Slug { get; set; }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != typeof(LanguageModel))
                    return false;
                var other = (LanguageModel)obj;
                return Name == other.Name && Slug == other.Slug;
            }
            

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Name != null ? Name.GetHashCode() : 0) ^ (Slug != null ? Slug.GetHashCode() : 0);
                }
            }
            
        }

        public class TimeModel
        {
            public string Name { get; set; }
            public string Slug { get; set; }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != typeof(TimeModel))
                    return false;
                var other = (TimeModel)obj;
                return Name == other.Name && Slug == other.Slug;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Name != null ? Name.GetHashCode() : 0) ^ (Slug != null ? Slug.GetHashCode() : 0);
                }
            }
            
        }

        public class RepositoryModel
        {
            public string Url { get; set; }
            public string Owner { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int Stars { get; set; }
            public int Forks { get; set; }
        }
    }
}

