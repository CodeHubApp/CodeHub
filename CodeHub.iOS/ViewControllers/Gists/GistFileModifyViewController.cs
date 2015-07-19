using System;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.iOS.DialogElements;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using UIKit;

namespace CodeHub.iOS.ViewControllers.Gists
{
    public abstract class GistFileModifyViewController<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : GistFileModifyViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.WhenAnyValue(x => x.ViewModel.SaveCommand)
                .Subscribe(x => {
                    NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Save, (s, e) => {
                        ResignFirstResponder();
                        x.ExecuteIfCan();
                    });
                    NavigationItem.RightBarButtonItem.EnableIfExecutable(x);
                });

            var titleElement = new DummyInputElement("Title");
            this.WhenAnyValue(x => x.ViewModel.Filename).Subscribe(x => titleElement.Value = x);
            titleElement.Changed += (sender, e) => ViewModel.Filename = titleElement.Value;

            var descriptionElement = new ExpandingInputElement("Description");
            this.WhenAnyValue(x => x.ViewModel.Description).Subscribe(x => descriptionElement.Value = x);
            descriptionElement.ValueChanged += (sender, e) => ViewModel.Description = descriptionElement.Value;

            var source = new DialogTableViewSource(TableView);
            source.Root.Add(new Section { titleElement, descriptionElement });
            TableView.Source = source;
            TableView.TableFooterView = new UIView();
        }
    }
}

