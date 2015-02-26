using System;
using CodeHub.Core.ViewModels.App;
using UIKit;
using CodeHub.iOS.ViewComponents;
using ReactiveUI;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.App
{
    public class FeedbackComposerView : BaseTableViewController<FeedbackComposerViewModel>, IModalView
    {
        private readonly DummyInputElement _titleElement = new DummyInputElement("Title");
        private readonly ExpandingInputElement _descriptionElement = new ExpandingInputElement("Description");

        public FeedbackComposerView()
        {
            _descriptionElement.AccessoryView = x => new MarkdownAccessoryView(x);

            this.WhenAnyValue(x => x.ViewModel.Subject).Subscribe(x => _titleElement.Value = x);
            _titleElement.Changed += (sender, e) => ViewModel.Subject = _titleElement.Value;

            this.WhenAnyValue(x => x.ViewModel.Description).Subscribe(x => _descriptionElement.Value = x);
            _descriptionElement.ValueChanged += (sender, e) => ViewModel.Description = _descriptionElement.Value;

            this.WhenAnyValue(x => x.ViewModel.SubmitCommand).Subscribe(x =>
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
