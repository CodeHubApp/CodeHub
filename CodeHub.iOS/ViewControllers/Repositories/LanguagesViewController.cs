using CodeHub.Core.ViewModels.Repositories;
using System;
using System.Reactive.Linq;
using System.Linq;
using CodeHub.iOS.TableViewSources;
using Foundation;
using ReactiveUI;

namespace CodeHub.iOS.ViewControllers.Repositories
{
    public class LanguagesViewController : BaseTableViewController<LanguagesViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.WhenAnyValue(x => x.ViewModel.Languages)
                .Select(x => {
                    var source = new LanguageTableViewSource(TableView, x);
                    source.ElementSelected.OfType<LanguageItemViewModel>().Subscribe(y => ViewModel.SelectedLanguage = y);
                    return source;
                }).BindTo(TableView, x => x.Source);

            ViewModel.LoadCommand.IsExecuting.Where(x => !x).Take(1).SubscribeSafe(_ => {
                var selectedLanguageSlug = ViewModel.SelectedLanguage.Slug;
                var selectedLanguage = ViewModel.Languages.Select((value, index) => new { value, index })
                    .Where(x => x.value.Slug == selectedLanguageSlug)
                    .Select(x => x.index + 1)
                    .FirstOrDefault() - 1;

                if (selectedLanguage >= 0)
                {
                    var indexPath = NSIndexPath.FromRowSection(selectedLanguage, 0);
                    BeginInvokeOnMainThread(() => TableView.ScrollToRow(indexPath, UIKit.UITableViewScrollPosition.Middle, true));
                }
            });
        }
    }
}