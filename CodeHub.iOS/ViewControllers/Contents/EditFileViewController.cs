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
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TextView.Font = UIFont.FromName("Courier", UIFont.PreferredBody.PointSize);

            this.WhenAnyValue(x => x.ViewModel.Text)
                .IsNotNull()
                .Take(1)
                .Subscribe(x => Text = x);
            
            this.WhenAnyValue(x => x.ViewModel.Text)
                .IsNotNull()
                .Skip(1)
                .Where(x => !string.Equals(x, TextView.Text))
                .Subscribe(x => TextView.Text = x);

            OnActivation(d => {
                d(this.WhenAnyValue(x => x.ViewModel.DismissCommand)
                    .ToBarButtonItem(Images.Cancel, x => NavigationItem.LeftBarButtonItem = x));

                d(this.WhenAnyValue(x => x.ViewModel.GoToCommitMessageCommand)
                    .ToBarButtonItem(UIBarButtonSystemItem.Save, x => NavigationItem.RightBarButtonItem = x));

                d(this.WhenAnyObservable(x => x.ViewModel.GoToCommitMessageCommand)
                    .Subscribe(_ => GoToMessage()));

                d(Changed.Subscribe(x => ViewModel.Text = x));
            });
        }

        private void GoToMessage()
        {
            ResignFirstResponder();

            var viewController = new MessageComposerViewController();
            viewController.Title = "Commit Message";

            viewController.OnActivation(d => {
                d(this.WhenAnyValue(x => x.ViewModel.CommitMessage).Subscribe(x => viewController.TextView.Text = x));
                d(viewController.Changed.Subscribe(x => ViewModel.CommitMessage = x));
                d(this.WhenAnyValue(x => x.ViewModel.SaveCommand)
                    .ToBarButtonItem(UIBarButtonSystemItem.Save, x => viewController.NavigationItem.RightBarButtonItem = x));
            });

            NavigationController.PushViewController(viewController, true);
        }
    }
}

