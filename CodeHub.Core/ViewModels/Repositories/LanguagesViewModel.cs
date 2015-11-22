using System;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive;
using CodeHub.Core.Data;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class LanguagesViewModel : BaseSearchableListViewModel<Language, LanguageItemViewModel>
    {
        private LanguageItemViewModel _selectedLanguage;
        public LanguageItemViewModel SelectedLanguage
        {
            get { return _selectedLanguage; }
            set { this.RaiseAndSetIfChanged(ref _selectedLanguage, value); }
        }

        public IReactiveCommand<object> DismissCommand { get; }

        public LanguagesViewModel()
        {
            Title = "Languages";

            DismissCommand = ReactiveCommand.Create();
            DismissCommand.Subscribe(_ => this.Dismiss());

            Items = InternalItems.CreateDerivedCollection(
                x => new LanguageItemViewModel(x.Name, x.Slug), 
                filter: x => x.Name.StartsWith(SearchKeyword ?? string.Empty, StringComparison.OrdinalIgnoreCase), 
                signalReset: this.WhenAnyValue(x => x.SearchKeyword));

            Items
                .Changed.Select(_ => Unit.Default)
                .Merge(this.WhenAnyValue(x => x.SelectedLanguage).Select(_ => Unit.Default))
                .Select(_ => SelectedLanguage)
                .Where(x => x != null)
                .Subscribe(x => {
                    foreach (var l in Items)
                        l.Selected = l.Slug == x.Slug;
                });

            this.WhenAnyValue(x => x.SelectedLanguage)
                .IsNotNull()
                .Subscribe(_ => Dismiss());

            LoadCommand = ReactiveCommand.CreateAsyncTask(async t => {
                var languageRepository = new LanguageRepository();
                var langs = await languageRepository.GetLanguages();
                langs.Insert(0, LanguageRepository.DefaultLanguage);
                InternalItems.Reset(langs);
            });
        }
    }
}