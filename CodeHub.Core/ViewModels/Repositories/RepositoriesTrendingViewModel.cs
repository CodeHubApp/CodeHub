using System;
using CodeFramework.Core.ViewModels;
using System.Windows.Input;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.Services;
using System.Collections.Generic;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesTrendingViewModel : LoadableViewModel
    {
        private const string LanguagesUrl = "http://trending.codehub-app.com/languages";
        private const string TrendingUrl = "http://trending.codehub-app.com/trending";
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

        private readonly CollectionViewModel<RepositoryModel> _repositories = new CollectionViewModel<RepositoryModel>();
        public CollectionViewModel<RepositoryModel> Repositories
        {
            get { return _repositories; }
        }

        private readonly CollectionViewModel<LanguageModel> _languages = new CollectionViewModel<LanguageModel>();
        public CollectionViewModel<LanguageModel> Languages
        {
            get { return _languages; }
        }

        private LanguageModel _selectedLanguage;
        public LanguageModel SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                if (object.Equals(_selectedLanguage, value))
                    return;
                _selectedLanguage = value;
                RaisePropertyChanged(() => SelectedLanguage);
            }
        }

        private TimeModel _selectedTime;
        public TimeModel SelectedTime
        {
            get { return _selectedTime; }
            set
            {
                if (object.Equals(_selectedTime, value))
                    return;
                _selectedTime = value;
                RaisePropertyChanged(() => SelectedTime);
            }
        }


        public bool ShowRepositoryDescription
        {
            get { return this.GetApplication().Account.ShowRepositoryDescriptionInList; }
        }

        public RepositoriesTrendingViewModel(IJsonHttpClientService jsonHttpClient)
        {
            _jsonHttpClient = jsonHttpClient;
        }

        public void Init()
        {
            SelectedTime = Times[0];
            SelectedLanguage = _defaultLanguage;
            GetLanguages().FireAndForget();
            this.Bind(x => x.SelectedTime, () => LoadCommand.Execute(null));
            this.Bind(x => x.SelectedLanguage, () => LoadCommand.Execute(null));
        }

        public ICommand GoToRepositoryCommand
        {
            get { return new MvxCommand<RepositoryModel>(x => ShowViewModel<RepositoryViewModel>(new RepositoryViewModel.NavObject { Username = x.Owner, Repository = x.Name })); }
        }
 
        protected override async Task Load(bool forceCacheInvalidation)
        {
            var query = "?";
            if (SelectedLanguage != null && SelectedLanguage.Slug != null)
                query += string.Format("language={0}&", SelectedLanguage.Slug);
            if (SelectedTime != null && SelectedTime.Slug != null)
                query += string.Format("since={0}", SelectedTime.Slug);

            var repos = await _jsonHttpClient.Get<List<RepositoryModel>>(TrendingUrl + query);
            Repositories.Items.Clear();
            Repositories.Items.Reset(repos);
        }


        private async Task GetLanguages()
        {
            var languages = await _jsonHttpClient.Get<List<LanguageModel>>(LanguagesUrl);
            languages.Insert(0, _defaultLanguage);
            Languages.Items.Reset(languages);
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
                LanguageModel other = (LanguageModel)obj;
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
                TimeModel other = (TimeModel)obj;
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

