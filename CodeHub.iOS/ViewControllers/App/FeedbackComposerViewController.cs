using System;
using CodeHub.Core.ViewModels.App;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.TableViewSources;
using System.Reactive.Linq;
using CodeHub.iOS.Views;
using System.Reactive;

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

            var saveButton = new UIBarButtonItem(UIBarButtonSystemItem.Save);
            var cancelButton = new UIBarButtonItem { Image = Images.Cancel };
            NavigationItem.LeftBarButtonItem = cancelButton;
            NavigationItem.RightBarButtonItem = saveButton;

            var source = new DialogTableViewSource(TableView);
            source.Root.Add(new Section { titleElement, descriptionElement });
            TableView.Source = source;
            TableView.TableFooterView = new UIView();

            OnActivation(d => {
                d(descriptionElement.Changed.Subscribe(x => ViewModel.Description = x));
                d(this.WhenAnyValue(x => x.ViewModel.Subject).Subscribe(x => titleElement.Value = x));
                d(this.WhenAnyValue(x => x.ViewModel.Description).Subscribe(x => descriptionElement.Value = x));
                d(titleElement.Changed.Subscribe(x => ViewModel.Subject = x));
                d(this.WhenAnyValue(x => x.ViewModel.DismissCommand)
                    .ToBarButtonItem(Images.Cancel, x => NavigationItem.LeftBarButtonItem = x));

                d(saveButton.GetClickedObservable()
                    .Do<Unit>(x => ResignFirstResponder())
                    .InvokeCommand(ViewModel.SubmitCommand));

                d(this.WhenAnyObservable(x => x.ViewModel.SubmitCommand.CanExecuteObservable)
                    .Subscribe(x => saveButton.Enabled = x));
            });
        }
    }
}
