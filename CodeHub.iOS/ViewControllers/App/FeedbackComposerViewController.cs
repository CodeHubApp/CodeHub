using System;
using CodeHub.Core.ViewModels.App;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.TableViewSources;
using System.Reactive.Linq;
using CodeHub.iOS.Views;

namespace CodeHub.iOS.ViewControllers.App
{
    public class FeedbackComposerViewController : BaseTableViewController<FeedbackComposerViewModel>, IModalViewController
    {
        public FeedbackComposerViewController()
        {
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
                ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var descriptionElement = new ExpandingInputElement("Description");
            descriptionElement.AccessoryView = x => new MarkdownAccessoryView(x);

            var titleElement = new DummyInputElement("Title");
            this.WhenAnyValue(x => x.ViewModel.Subject).Subscribe(x => titleElement.Value = x);
            titleElement.Changed += (sender, e) => ViewModel.Subject = titleElement.Value;

            this.WhenAnyValue(x => x.ViewModel.Description).Subscribe(x => descriptionElement.Value = x);
            descriptionElement.ValueChanged += (sender, e) => ViewModel.Description = descriptionElement.Value;

            this.WhenAnyValue(x => x.ViewModel.SubmitCommand).Subscribe(x => {
                NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Save, (s, e) => {
                    ResignFirstResponder();
                    x.ExecuteIfCan();
                });

                x.CanExecuteObservable.Subscribe(y => NavigationItem.RightBarButtonItem.Enabled = y);
            });

            this.WhenAnyValue(x => x.ViewModel.DismissCommand)
                .Select(x => x.ToBarButtonItem(Images.Cancel))
                .Subscribe(x => NavigationItem.LeftBarButtonItem = x);

            var source = new DialogTableViewSource(TableView);
            source.Root.Add(new Section { titleElement, descriptionElement });
            TableView.Source = source;
            TableView.TableFooterView = new UIView();
        }
    }
}
