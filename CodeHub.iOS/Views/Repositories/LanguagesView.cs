using ReactiveUI;
using CodeHub.Core.ViewModels.Repositories;
using System;
using System.Reactive.Linq;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.Languages
{
    public class LanguagesView : ReactiveTableViewController<LanguagesViewModel>
    {
        public override void ViewDidLoad()
        {
            Title = "Langauges";

            base.ViewDidLoad();

            this.AddSearchBar(x => ViewModel.SearchKeyword = x);

            var source = new LanguageTableViewSource(TableView, ViewModel.Languages);
            source.ElementSelected.OfType<LanguageItemViewModel>().Subscribe(x => ViewModel.SelectedLanguage = x);
            TableView.Source = source;
        }
    }
}