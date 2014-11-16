using System;
using ReactiveUI;
using Xamarin.Utilities.Core.ViewModels;
using System.Collections.Generic;
using Xamarin.Utilities.Core.Services;
using Akavache;
using CodeHub.Core.Models;
using System.Reactive.Linq;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class LanguagesViewModel : BaseViewModel, ILoadableViewModel
    {
        private const string LanguagesUrl = "http://trending.codehub-app.com/languages";
        public static LanguageModel DefaultLanguage = new LanguageModel { Name = "All Languages", Slug = null };

        public IReactiveCommand LoadCommand { get; private set; }

        private LanguageItemViewModel _selectedLanguage;
        public LanguageItemViewModel SelectedLanguage
        {
            get { return _selectedLanguage; }
            set { this.RaiseAndSetIfChanged(ref _selectedLanguage, value); }
        }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set { this.RaiseAndSetIfChanged(ref _searchKeyword, value); }
        }

        public IReadOnlyReactiveList<LanguageItemViewModel> Languages { get; private set; }

        public LanguagesViewModel(IJsonSerializationService jsonSerializationService, INetworkActivityService networkActivity)
        {
            Title = "Languages";

            var languages = new ReactiveList<LanguageModel>();

            Languages = languages.CreateDerivedCollection(
                x => new LanguageItemViewModel(x.Name, x.Slug), 
                filter: x => x.Name.StartsWith(SearchKeyword ?? string.Empty, StringComparison.OrdinalIgnoreCase), 
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            Languages
                .Changed.Select(_ => Unit.Default)
                .Merge(this.WhenAnyValue(x => x.SelectedLanguage).Select(_ => Unit.Default))
                .Select(_ => SelectedLanguage)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    foreach (var l in Languages)
                        l.Selected = l.Slug == x.Slug;
                });

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                var trendingData = await BlobCache.LocalMachine.DownloadUrl(LanguagesUrl, absoluteExpiration: DateTimeOffset.Now.AddDays(1));
                var langs = jsonSerializationService.Deserialize<List<LanguageModel>>(System.Text.Encoding.UTF8.GetString(trendingData));
                langs.Insert(0, DefaultLanguage);
                languages.Reset(langs);
            });
        }
    }
}