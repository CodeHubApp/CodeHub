using System;
using CodeHub.iOS.ViewControllers;
using MonoTouch.UIKit;
using CodeHub.Core.ViewModels.Contents;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.iOS.Views;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.Contents
{
    public class CreateFileView : BaseTableViewController<CreateFileViewModel>
    {
        private readonly DummyInputElement _titleElement = new DummyInputElement("Name");
        private readonly ExpandingInputElement _descriptionElement;

        public CreateFileView()
        {
            _titleElement.EntryFont = UIFont.SystemFontOfSize(14f);
            _titleElement.SpellChecking = false;

            _descriptionElement = new ExpandingInputElement("Content");
            _descriptionElement.Font = UIFont.FromName("Courier", 14f);
            _descriptionElement.SpellChecking = false;

            this.WhenViewModel(x => x.Name).Subscribe(x => _titleElement.Value = x);
            _titleElement.Changed += (sender, e) => ViewModel.Name = _titleElement.Value;

            this.WhenViewModel(x => x.Content).Subscribe(x => _descriptionElement.Value = x);
            _descriptionElement.ValueChanged += (sender, e) => ViewModel.Content = _descriptionElement.Value;

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Save, PromptForCommitMessage);
            this.WhenAnyValue(x => x.ViewModel.CanCommit).Subscribe(x => NavigationItem.RightBarButtonItem.Enabled = x);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new DialogTableViewSource(TableView, true);
            source.Root.Add(new Section { _titleElement, _descriptionElement });
            TableView.Source = source;
            TableView.TableFooterView = new UIView();
        }

        private void PromptForCommitMessage(object sender, EventArgs args)
        {
            ResignFirstResponder();

            var viewController = new MessageComposerViewController();
            viewController.Title = "Commit Message";
            viewController.TextView.Font = UIFont.SystemFontOfSize(16f);
            ViewModel.WhenAnyValue(x => x.CommitMessage).Subscribe(x => viewController.TextView.Text = x);
            viewController.TextView.Changed += (s, e) => ViewModel.CommitMessage = viewController.TextView.Text;
            viewController.NavigationItem.RightBarButtonItem = ViewModel.SaveCommand.ToBarButtonItem(UIBarButtonSystemItem.Save);
            NavigationController.PushViewController(viewController, true);
        }
    }
}

