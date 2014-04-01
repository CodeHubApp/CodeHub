using System;
using CodeHub.Core.ViewModels.PullRequests;
using MonoTouch.Dialog;
using CodeFramework.iOS.Utils;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestView : CodeHub.iOS.Views.Issues.IssueView
    {
		private SplitElement _split1, _split2;

        public new PullRequestViewModel ViewModel
        {
            get { return (PullRequestViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }
 
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _split1 = new SplitElement(new SplitElement.Row { Image1 = Images.Cog, Image2 = Images.Merge });
            _split2 = new SplitElement(new SplitElement.Row { Image1 = Images.Person, Image2 = Images.Create });

            ViewModel.Bind(x => x.PullRequest, () =>
            {
                var merged = (ViewModel.PullRequest.Merged != null && ViewModel.PullRequest.Merged.Value);

                _split1.Value.Text1 = ViewModel.PullRequest.State;
                _split1.Value.Text2 = merged ? "Merged" : "Not Merged";

                _split2.Value.Text1 = ViewModel.PullRequest.User.Login;
                _split2.Value.Text2 = ViewModel.PullRequest.CreatedAt.ToString("MM/dd/yy");
                Render();
            });
        }

		public override void ViewWillAppear(bool animated)
        {
			base.ViewWillAppear(animated);
            Title = "Pull Request #".t() + ViewModel.Id;
        }

        protected override void Render()
        {
            //Wait for the issue to load
            if (ViewModel.Issue == null || ViewModel.PullRequest == null)
                return;

            var root = new RootElement(Title);
            root.Add(new Section(_header));

            var secDetails = new Section();
            if (!string.IsNullOrEmpty(_descriptionElement.Value))
                secDetails.Add(_descriptionElement);

            secDetails.Add(_split1);
            secDetails.Add(_split2);

            secDetails.Add(_assigneeElement);
            secDetails.Add(_milestoneElement);
            secDetails.Add(_labelsElement);
            root.Add(secDetails);

            root.Add(new Section
            {
                new StyledStringElement("Commits", () => ViewModel.GoToCommitsCommand.Execute(null), Images.Commit),
                new StyledStringElement("Files", () => ViewModel.GoToFilesCommand.Execute(null), Images.File),
            });

            if (!(ViewModel.PullRequest.Merged != null && ViewModel.PullRequest.Merged.Value))
            {
                MonoTouch.Foundation.NSAction mergeAction = async () =>
                {
                    try
                    {
                        await this.DoWorkAsync("Merging...", ViewModel.Merge);
                    }
                    catch (Exception e)
                    {
                        MonoTouch.Utilities.ShowAlert("Unable to Merge", e.Message);
                    }
                };

                StyledStringElement el;
                if (ViewModel.PullRequest.Mergable == null)
                    el = new StyledStringElement("Merge".t(), mergeAction, Images.Fork);
                else if (ViewModel.PullRequest.Mergable.Value)
                    el = new StyledStringElement("Merge".t(), mergeAction, Images.Fork);
                else
                    el = new StyledStringElement("Unable to merge!".t()) { Image = Images.Fork };

                root.Add(new Section { el });
            }

            if (!string.IsNullOrEmpty(_commentsElement.Value))
                root.Add(new Section { _commentsElement });

            root.Add(new Section { _addCommentElement });


            Root = root;

        }
    }
}

