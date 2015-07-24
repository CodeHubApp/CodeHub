using System;
using CodeHub.iOS.ViewControllers;
using UIKit;
using CodeHub.Core.ViewModels.Contents;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.ViewControllers.Contents
{
    public class CreateFileViewController : BaseTableViewController<CreateFileViewModel>, IModalViewController
    {
        private readonly Lazy<MessageComposerViewController> _messageViewController;

        public CreateFileViewController()
        {
            _messageViewController = new Lazy<MessageComposerViewController>(() => {
                var viewController = new MessageComposerViewController();
                viewController.Title = "Commit Message";
                this.WhenAnyValue(x => x.ViewModel.CommitMessage)
                    .Subscribe(x => viewController.TextView.Text = x);
                this.WhenAnyValue(x => x.ViewModel.SaveCommand)
                    .Subscribe(x => viewController.NavigationItem.RightBarButtonItem = x.ToBarButtonItem(UIBarButtonSystemItem.Save));
                viewController.TextView.Changed += (s, e) => ViewModel.CommitMessage = viewController.TextView.Text;
                return viewController;
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var titleElement = new DummyInputElement("Name");
            titleElement.SpellChecking = false;

            var descriptionElement = new ExpandingInputElement("Content");
            descriptionElement.Font = UIFont.FromName("Courier", UIFont.PreferredBody.PointSize);
            descriptionElement.SpellChecking = false;

            this.WhenAnyValue(x => x.ViewModel.Name)
                .Subscribe(x => titleElement.Value = x);
            titleElement.Changed += (sender, e) => ViewModel.Name = titleElement.Value;

            this.WhenAnyValue(x => x.ViewModel.Content)
                .Subscribe(x => descriptionElement.Value = x);
            descriptionElement.ValueChanged += (sender, e) => ViewModel.Content = descriptionElement.Value;

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Save, (s, e) => {
                ResignFirstResponder();
                NavigationController.PushViewController(_messageViewController.Value, true);
            });
            this.WhenAnyValue(x => x.ViewModel.CanCommit)
                .Subscribe(x => NavigationItem.RightBarButtonItem.Enabled = x);

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

