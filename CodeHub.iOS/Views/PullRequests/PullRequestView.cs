using System;
using CodeHub.Core.ViewModels.PullRequests;
using ReactiveUI;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.Views.Issues;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestView : BaseIssueView<PullRequestViewModel>
    {
        private readonly SplitViewElement _split1, _split2;
        private readonly StringElement _commitsElement;
        private readonly StringElement _filesElement;

        public PullRequestView()
        {
            _split1 = new SplitViewElement
            {
                Button1 = new SplitViewElement.SplitButton(Images.Gear, string.Empty),
                Button2 = new SplitViewElement.SplitButton(Images.Gear, string.Empty)
            };

            _split2 = new SplitViewElement
            {
                Button1 = new SplitViewElement.SplitButton(Images.Gear, string.Empty),
                Button2 = new SplitViewElement.SplitButton(Images.Gear, string.Empty)
            };

            _commitsElement = new StringElement("Commits", string.Empty) 
            { 
                Image = Images.Commit,
                Tapped = () => ViewModel.GoToCommitsCommand.ExecuteIfCan()
            };

            _filesElement = new StringElement("Files", string.Empty)
            {
                Image = Images.Code,
                Tapped = () => ViewModel.GoToFilesCommand.ExecuteIfCan()
            };

            this.WhenAnyValue(x => x.ViewModel.PullRequest.Commits)
                .Subscribe(x => _commitsElement.Value = x.ToString());

            this.WhenAnyValue(x => x.ViewModel.PullRequest.ChangedFiles)
                .Subscribe(x => _filesElement.Value = x.ToString());
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            DetailsSection.Insert(0, _split1);
            DetailsSection.Insert(1, _split2);

            Root.Insert(Root.Count - 1, new Section { _commitsElement, _filesElement });
        }

        private void Render()
        {

//            if (!(ViewModel.PullRequest.Merged != null && ViewModel.PullRequest.Merged.Value))
//            {
//                Action mergeAction = async () =>
//                {
////                    try
////                    {
////                        await this.DoWorkAsync("Merging...", ViewModel.Merge);
////                    }
////                    catch (Exception e)
////                    {
////                        MonoTouch.Utilities.ShowAlert("Unable to Merge", e.Message);
////                    }
//                };
//
////                StyledStringElement el;
////                if (ViewModel.PullRequest.Mergable == null)
////                    el = new StyledStringElement("Merge", mergeAction, Images.Fork);
////                else if (ViewModel.PullRequest.Mergable.Value)
////                    el = new StyledStringElement("Merge", mergeAction, Images.Fork);
////                else
////                    el = new StyledStringElement("Unable to merge!") { Image = Images.Fork };
////
//                sections.Add(new Section { el });
//            }
//
//            if (!string.IsNullOrEmpty(_commentsElement.Value))
//                root.Add(new Section { _commentsElement });
        }
    }
}

