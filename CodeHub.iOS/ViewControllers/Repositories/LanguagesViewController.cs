using CodeHub.Core.ViewModels.Repositories;
using System;
using System.Reactive.Linq;
using System.Linq;
using CodeHub.iOS.TableViewSources;
using Foundation;
using ReactiveUI;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Repositories
{
    public class LanguagesViewController : BaseTableViewController<LanguagesViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new LanguageTableViewSource(TableView, ViewModel.Items);
            TableView.Source = source;

            OnActivation(d => {
                d(this.WhenAnyObservable(x => x.ViewModel.LoadCommand.IsExecuting)
                    .Where(x => !x).Take(1).SubscribeSafe(_ => ScrollToSelected()));

                d(source.ElementSelected.OfType<LanguageItemViewModel>()
                    .Subscribe(y => ViewModel.SelectedLanguage = y));
                
                d(this.WhenAnyValue(x => x.ViewModel.DismissCommand)
                    .ToBarButtonItem(UIBarButtonSystemItem.Done, x => NavigationItem.LeftBarButtonItem = x));
            });
        }

        private void ScrollToSelected()
        {
            var selectedLanguageSlug = ViewModel.SelectedLanguage.Slug;
            var selectedLanguage = ViewModel.Items.Select((value, index) => new { value, index })
                .Where(x => x.value.Slug == selectedLanguageSlug)
                .Select(x => x.index + 1)
                .FirstOrDefault() - 1;

            if (selectedLanguage >= 0)
            {
                var indexPath = NSIndexPath.FromRowSection(selectedLanguage, 0);
                BeginInvokeOnMainThread(() => TableView.ScrollToRow(indexPath, UITableViewScrollPosition.Middle, true));
            }
        }
    }
}