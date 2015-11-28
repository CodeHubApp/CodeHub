using System;
using CodeHub.Core.ViewModels.PullRequests;
using ReactiveUI;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.ViewControllers.Issues;
using CodeHub.iOS.ViewControllers;
using UIKit;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace CodeHub.iOS.ViewControllers.PullRequests
{
    public class PullRequestViewController : BaseIssueViewController<PullRequestViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var commitsElement = new StringElement("Commits", string.Empty) { Image = Octicon.GitCommit.ToImage() };
            var filesElement = new StringElement("Files Changed", string.Empty) { Image = Octicon.Diff.ToImage() };
            var mergeButton = new StringElement("Merge", Octicon.GitMerge.ToImage());
            var pullRequestSection = new Section { commitsElement, filesElement, mergeButton };

            OnActivation(d => {
                var commitsObs = this.WhenAnyValue(x => x.ViewModel.PullRequest.Commits);
                d(commitsElement.BindValue(commitsObs));
                d(commitsElement.BindCommand(ViewModel.GoToCommitsCommand));
                d(commitsElement.BindDisclosure(commitsObs.Select(x => x > 0)));

                var filesObs = this.WhenAnyValue(x => x.ViewModel.PullRequest.ChangedFiles);
                d(filesElement.BindValue(filesObs));
                d(filesElement.BindCommand(ViewModel.GoToFilesCommand));
                d(filesElement.BindDisclosure(filesObs.Select(x => x > 0)));

                d(mergeButton.Clicked.SubscribeSafe(_ => {
                    if (!ViewModel.PullRequest.Merged && ViewModel.CanMerge && ViewModel.PullRequest.State == Octokit.ItemState.Open)
                        PromptForCommitMessage();
                }));

                d(this.WhenAnyValue(x => x.ViewModel.PullRequest.Merged, x => x.ViewModel.CanMerge, x => x.ViewModel.PullRequest.State)
                    .Subscribe(x => {
                        if (x.Item1)
                        {
                            mergeButton.Caption = "Merged";
                            mergeButton.SelectionStyle = UITableViewCellSelectionStyle.None;
                            mergeButton.Accessory = UITableViewCellAccessory.Checkmark;
                        }
                        else if (!x.Item1 && x.Item2 && x.Item3 == Octokit.ItemState.Open)
                        {
                            mergeButton.Caption = "Merge";
                            mergeButton.SelectionStyle = null;
                            mergeButton.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                        }
                        else
                        {
                            mergeButton.Caption = "Not Merged";
                            mergeButton.SelectionStyle = UITableViewCellSelectionStyle.None;
                            mergeButton.Accessory = UITableViewCellAccessory.None;
                        }
                    }));
            });

            Root.Insert(Root.Count - 1, pullRequestSection);
        }

        private void PromptForCommitMessage()
        {
            var confirmButton = new UIBarButtonItem { Title = "Confirm" };
            var cancelButton = new UIBarButtonItem { Image = Images.Cancel };
            var viewController = new MessageComposerViewController();
            viewController.Title = "Message";
            viewController.TextView.Placeholder = "Merge Message (Optional)";

            viewController.OnActivation(d => {
                d(ViewModel.WhenAnyValue(x => x.MergeComment).Subscribe(x => viewController.TextView.Text = x));
                d(viewController.TextView.GetChangedObservable().Subscribe(x => ViewModel.MergeComment = x));

                viewController.NavigationItem.RightBarButtonItem = confirmButton;
                d(ViewModel.MergeCommand.CanExecuteObservable.Subscribe(x => confirmButton.Enabled = x));
                d(confirmButton.GetClickedObservable().InvokeCommand(ViewModel.MergeCommand));
                d(ViewModel.MergeCommand.Subscribe(_ => viewController.DismissViewController(true, null)));
                d(Disposable.Create(() => viewController.NavigationItem.RightBarButtonItem = null));

                viewController.NavigationItem.LeftBarButtonItem = cancelButton;
                d(cancelButton.GetClickedObservable().Subscribe(_ => viewController.DismissViewController(true, null)));
                d(Disposable.Create(() => viewController.NavigationItem.LeftBarButtonItem = null));
            });

            PresentViewController(new ThemedNavigationController(viewController), true, null);
        }
    }
}

