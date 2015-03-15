using System;
using CodeHub.Core.ViewModels.PullRequests;
using ReactiveUI;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.Views.Issues;
using CodeHub.iOS.ViewControllers;
using UIKit;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestView : BaseIssueView<PullRequestViewModel>
    {
        private readonly StringElement _commitsElement;
        private readonly StringElement _filesElement;
        private readonly StringElement _mergeButton;
        private readonly Section _pullRequestSection;

        public PullRequestView()
        {
            _commitsElement = new StringElement("Commits", string.Empty) 
            { 
                Image = Octicon.GitCommit.ToImage(),
                Tapped = () => ViewModel.GoToCommitsCommand.ExecuteIfCan()
            };

            _filesElement = new StringElement("Files", string.Empty)
            {
                Image = Octicon.FileCode.ToImage(),
                Tapped = () => ViewModel.GoToFilesCommand.ExecuteIfCan()
            };

            _mergeButton = new StringElement("Merge", string.Empty)
            {
                Image = Octicon.GitMerge.ToImage(),
            };

            _pullRequestSection = new Section { _commitsElement, _filesElement, _mergeButton };

            this.WhenAnyValue(x => x.ViewModel.PullRequest.Commits)
                .Subscribe(x => _commitsElement.Value = x.ToString());

            this.WhenAnyValue(x => x.ViewModel.PullRequest.ChangedFiles)
                .Subscribe(x => _filesElement.Value = x.ToString());

            this.WhenAnyValue(x => x.ViewModel.Merged, x => x.ViewModel.CanMerge)
                .Subscribe(x => {
                    if (x.Item1)
                    {
                        _mergeButton.Caption = "Merged";
                        _mergeButton.Tapped = null;
                        _mergeButton.Accessory = UITableViewCellAccessory.Checkmark;
                    }
                    else if (!x.Item1 && x.Item2)
                    {
                        _mergeButton.Caption = "Merge";
                        _mergeButton.Tapped = PromptForCommitMessage;
                        _mergeButton.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                    }
                    else
                    {
                        _mergeButton.Caption = "Not Merged";
                        _mergeButton.Tapped = null;
                        _mergeButton.Accessory = UITableViewCellAccessory.None;
                    }
                });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Root.Insert(Root.Count - 1, _pullRequestSection);
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

