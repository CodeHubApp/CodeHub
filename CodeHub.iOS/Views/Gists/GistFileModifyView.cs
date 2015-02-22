using System;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.iOS.DialogElements;
using ReactiveUI;
using CodeHub.iOS.TableViewSources;
using UIKit;

namespace CodeHub.iOS.Views.Gists
{
    public abstract class GistFileModifyView<TViewModel> : BaseTableViewController<TViewModel> where TViewModel : GistFileModifyViewModel
    {
        private readonly DummyInputElement _titleElement = new DummyInputElement("Title");
        private readonly ExpandingInputElement _descriptionElement = new ExpandingInputElement("Description");

        protected GistFileModifyView()
        {
            this.WhenAnyValue(x => x.ViewModel.Filename).Subscribe(x => _titleElement.Value = x);
            _titleElement.Changed += (sender, e) => ViewModel.Filename = _titleElement.Value;

            this.WhenAnyValue(x => x.ViewModel.Description).Subscribe(x => _descriptionElement.Value = x);
            _descriptionElement.ValueChanged += (sender, e) => ViewModel.Description = _descriptionElement.Value;

            this.WhenAnyValue(x => x.ViewModel.SaveCommand).Subscribe(x =>
            {
                NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Save, (s, e) =>
                {
                    ResignFirstResponder();
                    x.ExecuteIfCan();
                });

                x.CanExecuteObservable.Subscribe(y => NavigationItem.RightBarButtonItem.Enabled = y);
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new DialogTableViewSource(TableView);
            source.Root.Add(new Section { _titleElement, _descriptionElement });
            TableView.Source = source;
            TableView.TableFooterView = new UIView();
        }
    }
}

