using CodeHub.Core.ViewModels.Repositories;
using System;
using System.Reactive.Linq;
using System.Linq;
using CodeHub.iOS.TableViewSources;
using Xamarin.Utilities.ViewControllers;
using MonoTouch.Foundation;

namespace CodeHub.iOS.Views.Repositories
{
    public class LanguagesView : ReactiveTableViewController<LanguagesViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new LanguageTableViewSource(TableView, ViewModel.Languages);
            source.ElementSelected.OfType<LanguageItemViewModel>().Subscribe(x => ViewModel.SelectedLanguage = x);
            TableView.Source = source;

            // Loading is assumed to already have begun
            ViewModel.LoadCommand.IsExecuting.Where(x => !x).Take(1).SubscribeSafe(_ =>
            {
                var selectedLanguageSlug = ViewModel.SelectedLanguage.Slug;
                var selectedLanguage = ViewModel.Languages.Select((value, index) => new { value, index })
                    .Where(x => x.value.Slug == selectedLanguageSlug)
                    .Select(x => x.index + 1)
                    .FirstOrDefault() - 1;

                if (selectedLanguage >= 0)
                {
                    var indexPath = NSIndexPath.FromRowSection(selectedLanguage, 0);
                    TableView.ScrollToRow(indexPath, MonoTouch.UIKit.UITableViewScrollPosition.Top, false);
                }
            });
        }
    }
}