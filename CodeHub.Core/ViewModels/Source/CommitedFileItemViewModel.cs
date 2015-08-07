using System;
using ReactiveUI;
using System.Linq;
using Humanizer;

namespace CodeHub.Core.ViewModels.Source
{
    public class CommitedFileItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Name { get; private set; }

        public string RootPath { get; private set; }

        public string Ref { get; private set; }

        public string Subtitle { get; private set; }

        public IReactiveCommand<object> GoToCommand { get; private set; }

        private void CalculateSubtitle(int additions, int deletions, int changes, int comments = 0)
        {
            var subtitle = string.Empty;

            if (additions == changes && deletions == 0)
            {
                subtitle = "additions".ToQuantity(additions);
            }
            else if (deletions == changes && additions == 0)
            {
                subtitle = "deletions".ToQuantity(deletions);
            }
            else
            {
                subtitle = string.Format("{0}, {1}", "additions".ToQuantity(additions), "deletions".ToQuantity(deletions));
            }

            if (comments > 0)
            {
                subtitle = string.Format("{0}, {1}", subtitle, "comments".ToQuantity(comments));
            }

            Subtitle = subtitle;
        }

        private void CalculateRef(string contentsUrl)
        {
            try
            {
                var queryString = contentsUrl.Substring(contentsUrl.IndexOf('?') + 1);
                var args = queryString.Split('#')
                    .Select(x => x.Split('='))
                    .ToDictionary(x => x[0].ToLower(), x => x[1]);
                Ref = args["ref"];
            }
            catch
            {
            }
        }

        private CommitedFileItemViewModel(Action<CommitedFileItemViewModel> gotoAction)
        {
            GoToCommand = ReactiveCommand.Create().WithSubscription(_ => gotoAction(this));
        }

        internal CommitedFileItemViewModel(Octokit.PullRequestFile file, int comments, Action<CommitedFileItemViewModel> gotoAction)
            : this(gotoAction)
        {
            Name = System.IO.Path.GetFileName(file.FileName);
            RootPath = file.FileName.Substring(0, file.FileName.Length - Name.Length);
            CalculateSubtitle(file.Additions, file.Deletions, file.Changes, comments);
            CalculateRef(file.ContentsUrl.AbsoluteUri);
        }

        internal CommitedFileItemViewModel(Octokit.GitHubCommitFile file, Action<CommitedFileItemViewModel> gotoAction)
            : this(gotoAction)
        {
            Name = System.IO.Path.GetFileName(file.Filename);
            RootPath = file.Filename.Substring(0, file.Filename.Length - Name.Length);
            CalculateSubtitle(file.Additions, file.Deletions, file.Changes);
            CalculateRef(file.ContentsUrl);
        }
    }
}

