using System;
using ReactiveUI;
using System.Linq;
using Humanizer;

namespace CodeHub.Core.ViewModels.Source
{
    public class CommitedFileItemViewModel : ReactiveObject, ICanGoToViewModel
    {
        public string Name { get; }

        public string RootPath { get; }

        public string Ref { get; }

        public string Subtitle { get; }

        public IReactiveCommand<object> GoToCommand { get; }

        private static string CalculateSubtitle(int additions, int deletions, int changes, int comments = 0)
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

            return subtitle;
        }

        private static string CalculateRef(string contentsUrl)
        {
            try
            {
                var queryString = contentsUrl.Substring(contentsUrl.IndexOf('?') + 1);
                var args = queryString.Split('#')
                    .Select(x => x.Split('='))
                    .ToDictionary(x => x[0].ToLower(), x => x[1]);
                return args["ref"];
            }
            catch
            {
                return null;
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
            Subtitle = CalculateSubtitle(file.Additions, file.Deletions, file.Changes, comments);
            Ref = CalculateRef(file.ContentsUrl.AbsoluteUri);
        }

        internal CommitedFileItemViewModel(Octokit.GitHubCommitFile file, Action<CommitedFileItemViewModel> gotoAction)
            : this(gotoAction)
        {
            Name = System.IO.Path.GetFileName(file.Filename);
            RootPath = file.Filename.Substring(0, file.Filename.Length - Name.Length);
            Subtitle = CalculateSubtitle(file.Additions, file.Deletions, file.Changes);
            Ref = CalculateRef(file.ContentsUrl);
        }
    }
}

