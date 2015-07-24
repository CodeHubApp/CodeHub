using System;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.ViewControllers;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Contents;

namespace CodeHub.iOS.ViewControllers.Contents
{
    public class EditFileViewController : MessageComposerViewController<EditFileViewModel>, IModalViewController
    {
        private readonly Lazy<MessageComposerViewController> _messageViewController;

        public EditFileViewController()
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

            NavigationItem.RightBarButtonItem = new UIBarButtonItem(Images.SaveButton, UIBarButtonItemStyle.Plain, (s, e) => {
                ResignFirstResponder();
                NavigationController.PushViewController(_messageViewController.Value, true);
            });

            TextView.Font = UIFont.FromName("Courier", UIFont.PreferredBody.PointSize);
            TextView.Changed += (sender, e) => ViewModel.Text = Text;

            this.WhenAnyValue(x => x.ViewModel.Text)
                .IsNotNull()
                .Take(1)
                .Subscribe(x => Text = x);
            
            this.WhenAnyValue(x => x.ViewModel.Text)
                .IsNotNull()
                .Skip(1)
                .Where(x => !string.Equals(x, TextView.Text))
                .Subscribe(x => TextView.Text = x);

            this.WhenAnyValue(x => x.ViewModel.DismissCommand)
                .Select(x => x.ToBarButtonItem(Images.Cancel))
                .Subscribe(x => NavigationItem.LeftBarButtonItem = x);
        }
    }
}

