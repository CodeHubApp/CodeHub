using System;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.iOS.DialogElements;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using UIKit;
using System.Reactive;
using System.Reactive.Linq;

namespace CodeHub.iOS.ViewControllers.Gists
{
    public abstract class GistFileModifyViewController<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : GistFileModifyViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
   
            var titleElement = new DummyInputElement("Title");
            var descriptionElement = new ExpandingInputElement("Description");

            var source = new DialogTableViewSource(TableView);
            source.Root.Add(new Section { titleElement, descriptionElement });
            TableView.Source = source;
            TableView.TableFooterView = new UIView();

            OnActivation(d => {
                d(this.WhenAnyValue(x => x.ViewModel.Filename).Subscribe(x => titleElement.Value = x));
                d(titleElement.Changed.Subscribe(x => ViewModel.Filename = x));

                d(this.WhenAnyValue(x => x.ViewModel.Description).Subscribe(x => descriptionElement.Value = x));
                d(descriptionElement.Changed.Subscribe(x => ViewModel.Description = x));

                d(this.WhenAnyValue(x => x.ViewModel.SaveCommand)
                    .Do((IReactiveCommand<Unit> _) => ResignFirstResponder())
                    .ToBarButtonItem(UIBarButtonSystemItem.Save, x => NavigationItem.RightBarButtonItem = x));
            });
        }
    }
}

