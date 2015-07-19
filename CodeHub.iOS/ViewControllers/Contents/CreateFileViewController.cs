using System;
using CodeHub.iOS.ViewControllers;
using UIKit;
using CodeHub.Core.ViewModels.Contents;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.iOS.ViewControllers;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.ViewControllers.Contents
{
    public class CreateFileViewController : BaseTableViewController<CreateFileViewModel>, IModalViewController
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var titleElement = new DummyInputElement("Name");
            titleElement.SpellChecking = false;

            var descriptionElement = new ExpandingInputElement("Content");
            descriptionElement.Font = UIFont.FromName("Courier", UIFont.PreferredBody.PointSize);
            descriptionElement.SpellChecking = false;

            this.WhenAnyValue(x => x.ViewModel.Name).Subscribe(x => titleElement.Value = x);
            titleElement.Changed += (sender, e) => ViewModel.Name = titleElement.Value;

            this.WhenAnyValue(x => x.ViewModel.Content).Subscribe(x => descriptionElement.Value = x);
            descriptionElement.ValueChanged += (sender, e) => ViewModel.Content = descriptionElement.Value;

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Save, PromptForCommitMessage);
            this.WhenAnyValue(x => x.ViewModel.CanCommit).Subscribe(x => NavigationItem.RightBarButtonItem.Enabled = x);

            this.WhenAnyValue(x => x.ViewModel.DismissCommand)
                .Select(x => x.ToBarButtonItem(Images.Cancel))
                .Subscribe(x => NavigationItem.LeftBarButtonItem = x);

            var source = new DialogTableViewSource(TableView);
            source.Root.Add(new Section { titleElement, descriptionElement });
            TableView.Source = source;
            TableView.TableFooterView = new UIView();
        }

        private void PromptForCommitMessage(object sender, EventArgs args)
        {
            ResignFirstResponder();

            var viewController = new MessageComposerViewController();
            viewController.Title = "Commit Message";
            ViewModel.WhenAnyValue(x => x.CommitMessage).Subscribe(x => viewController.TextView.Text = x);
            viewController.TextView.Changed += (s, e) => ViewModel.CommitMessage = viewController.TextView.Text;
            viewController.NavigationItem.RightBarButtonItem = ViewModel.SaveCommand.ToBarButtonItem(UIBarButtonSystemItem.Save);
            NavigationController.PushViewController(viewController, true);
        }
    }
}

