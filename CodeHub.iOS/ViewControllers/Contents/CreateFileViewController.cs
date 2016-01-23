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
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var titleElement = new DummyInputElement("Name") { SpellChecking = false };
            var descriptionElement = new ExpandingInputElement("Content") { 
                SpellChecking = false, 
                Font = UIFont.FromName("Courier", UIFont.PreferredBody.PointSize)
            };

            var source = new DialogTableViewSource(TableView);
            source.Root.Add(new Section { titleElement, descriptionElement });
            TableView.Source = source;
            TableView.TableFooterView = new UIView();

            OnActivation(d => {
                d(this.WhenAnyValue(x => x.ViewModel.Name).Subscribe(x => titleElement.Value = x));
                d(this.WhenAnyValue(x => x.ViewModel.Content).Subscribe(x => descriptionElement.Value = x));

                d(titleElement.Changed.Subscribe(x => ViewModel.Name = x));
                d(descriptionElement.Changed.Subscribe(x => ViewModel.Content = x));

                d(this.WhenAnyValue(x => x.ViewModel.GoToCommitMessageCommand)
                    .ToBarButtonItem(UIBarButtonSystemItem.Save, x => NavigationItem.RightBarButtonItem = x));

                d(this.WhenAnyObservable(x => x.ViewModel.GoToCommitMessageCommand).Subscribe(_ => GoToMessage()));

                d(this.WhenAnyValue(x => x.ViewModel.DismissCommand)
                    .ToBarButtonItem(Images.Cancel, x => NavigationItem.LeftBarButtonItem = x));
            });
        }

        private void GoToMessage()
        {
            ResignFirstResponder();

            var viewController = new MessageComposerViewController();
            viewController.Title = "Commit Message";

            viewController.OnActivation(d => {
                d(this.WhenAnyValue(x => x.ViewModel.CommitMessage).Subscribe(x => viewController.TextView.Text = x));
                d(viewController.TextView.GetChangedObservable().Subscribe(x => ViewModel.CommitMessage = x));
                d(this.WhenAnyValue(x => x.ViewModel.SaveCommand)
                    .ToBarButtonItem(UIBarButtonSystemItem.Save, x => viewController.NavigationItem.RightBarButtonItem = x));
            });

            NavigationController.PushViewController(viewController, true);
        }
    }
}

