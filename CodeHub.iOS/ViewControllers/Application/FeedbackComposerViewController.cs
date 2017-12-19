using System;
using CodeHub.Core.ViewModels.App;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.TableViewSources;
using System.Reactive.Linq;
using CodeHub.iOS.Views;
using System.Reactive;
using CodeHub.iOS.Utilities;

namespace CodeHub.iOS.ViewControllers.Application
{
    public class FeedbackComposerViewController : TableViewController
    {
        public FeedbackComposerViewModel ViewModel { get; } = new FeedbackComposerViewModel();

        public FeedbackComposerViewController() : base(UITableViewStyle.Plain)
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
            var cancelButton = new UIBarButtonItem(UIBarButtonSystemItem.Cancel);
            NavigationItem.LeftBarButtonItem = cancelButton;
            NavigationItem.RightBarButtonItem = saveButton;

            var source = new DialogTableViewSource(TableView);
            source.Root.Add(new Section { titleElement, descriptionElement });
            TableView.Source = source;
            TableView.TableFooterView = new UIView();

            OnActivation(d => {
                d(descriptionElement.Changed
                  .Subscribe(x => ViewModel.Description = x));

                d(this.WhenAnyValue(x => x.ViewModel.Title)
                  .Subscribe(title => Title = title));

                d(this.WhenAnyValue(x => x.ViewModel.Subject)
                  .Subscribe(x => titleElement.Value = x));

                d(this.WhenAnyValue(x => x.ViewModel.Description)
                  .Subscribe(x => descriptionElement.Value = x));

                d(titleElement.Changed.Subscribe(x => ViewModel.Subject = x));

                d(cancelButton.GetClickedObservable()
                  .Select(_ => Unit.Default)
                  .InvokeReactiveCommand(ViewModel.DismissCommand));

	            d(this.WhenAnyObservable(x => x.ViewModel.DismissCommand)
	              .Where(x => x)
                  .Subscribe(_ => DismissViewController(true, null)));

                d(saveButton.GetClickedObservable()
                  .Do(x => ResignFirstResponder())
                  .Select(_ => Unit.Default)
                  .InvokeReactiveCommand(ViewModel.SubmitCommand));

                d(ViewModel.SubmitCommand
                  .Subscribe(_ => DismissViewController(true, null)));

                d(this.WhenAnyObservable(x => x.ViewModel.SubmitCommand.CanExecute)
                  .Subscribe(x => saveButton.Enabled = x));

                d(this.WhenAnyObservable(x => x.ViewModel.SubmitCommand.IsExecuting)
                  .SubscribeStatus("Submitting feedback"));
            });
        }
    }
}
