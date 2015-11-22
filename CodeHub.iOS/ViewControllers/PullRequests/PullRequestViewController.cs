using System;
using CodeHub.Core.ViewModels.PullRequests;
using ReactiveUI;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.ViewControllers.Issues;
using CodeHub.iOS.ViewControllers;
using System.Reactive.Linq;
using UIKit;
using System.Threading.Tasks;

namespace CodeHub.iOS.ViewControllers.PullRequests
{
    public class PullRequestViewController : BaseIssueViewController<PullRequestViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var commitsElement = new StringElement("Commits", Octicon.GitCommit.ToImage());
            var filesElement = new StringElement("Files Changed", Octicon.Diff.ToImage());
            var mergeButton = new StringElement("Merge", Octicon.GitMerge.ToImage());
            var pullRequestSection = new Section { commitsElement, filesElement, mergeButton };

            OnActivation(d => {
                d(this.WhenAnyValue(x => x.ViewModel.PullRequest.Commits)
                    .Subscribe(x => commitsElement.Value = x.ToString()));

                d(this.WhenAnyValue(x => x.ViewModel.PullRequest.ChangedFiles)
                    .Subscribe(x => filesElement.Value = x.ToString()));

                d(commitsElement.Clicked.InvokeCommand(ViewModel.GoToCommitsCommand));
                d(filesElement.Clicked.InvokeCommand(ViewModel.GoToFilesCommand));

                d(this.WhenAnyValue(x => x.ViewModel.PullRequest.Merged, x => x.ViewModel.CanMerge, x => x.ViewModel.PullRequest.State)
                    .Subscribe(x => {
    //                    if (x.Item1)
    //                    {
    //                        mergeButton.Caption = "Merged";
    //                        mergeButton.Tapped = null;
    //                        mergeButton.Accessory = UITableViewCellAccessory.Checkmark;
    //                    }
    //                    else if (!x.Item1 && x.Item2 && x.Item3 == Octokit.ItemState.Open)
    //                    {
    //                        mergeButton.Caption = "Merge";
    //                        mergeButton.Tapped = PromptForCommitMessage;
    //                        mergeButton.Accessory = UITableViewCellAccessory.DisclosureIndicator;
    //                    }
    //                    else
    //                    {
    //                        mergeButton.Caption = "Not Merged";
    //                        mergeButton.Tapped = null;
    //                        mergeButton.Accessory = UITableViewCellAccessory.None;
    //                    }
                    }));
            });

            Root.Insert(Root.Count - 1, pullRequestSection);
        }

        private void PromptForCommitMessage()
        {
            var viewController = new MessageComposerViewController();
            viewController.Title = "Message";
            ViewModel.WhenAnyValue(x => x.MergeComment).Subscribe(x => viewController.TextView.Text = x);
            viewController.TextView.Placeholder = "Merge Message (Optional)";
            viewController.TextView.Changed += (s, e) => ViewModel.MergeComment = viewController.TextView.Text;
            viewController.NavigationItem.RightBarButtonItem = new UIBarButtonItem("Confirm", UIBarButtonItemStyle.Done, (s, e) => MergeAndDismiss());
            viewController.NavigationItem.RightBarButtonItem.EnableIfExecutable(ViewModel.MergeCommand);
            viewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Cancel, UIBarButtonItemStyle.Done, (s, e) => DismissViewController(true, null));
            PresentViewController(new ThemedNavigationController(viewController), true, null);
        }

        private async Task MergeAndDismiss()
        {
            await ViewModel.MergeCommand.ExecuteAsync();
            await DismissViewControllerAsync(true);
        }
    }
}

