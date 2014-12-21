using System;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive;
using CodeHub.Core.Data;
using Xamarin.Utilities.ViewModels;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class LanguagesViewModel : BaseViewModel, ILoadableViewModel, IProvidesSearchKeyword
    {
        public IReactiveCommand<Unit> LoadCommand { get; private set; }

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

        public LanguagesViewModel()
        {
            Title = "Languages";

            var languages = new ReactiveList<Language>();
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

            this.WhenAnyValue(x => x.SelectedLanguage)
                .IsNotNull()
                .Subscribe(_ => Dismiss());

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t =>
            {
                var languageRepository = new LanguageRepository();
                var langs = await languageRepository.GetLanguages();
                langs.Insert(0, LanguageRepository.DefaultLanguage);
                languages.Reset(langs);
            });
        }
    }
}