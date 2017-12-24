using System;
using Foundation;
using CodeHub.iOS.Cells;
using UIKit;
using CodeHub.Core.Utilities;

namespace CodeHub.iOS.DialogElements
{
    public class PullRequestElement : Element
    {   
        private readonly Action _action;    
        private readonly Octokit.PullRequest _model;

        public PullRequestElement(Octokit.PullRequest model, Action action)
        {
            _model = model;
            _action = action;
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            var c = tv.DequeueReusableCell(PullRequestCellView.Key) as PullRequestCellView ?? PullRequestCellView.Create();
            c.Set(_model.Title, _model.CreatedAt, new GitHubAvatar(_model.User?.AvatarUrl));
            return c;
        }

        public override bool Matches(string text)
        {
            return _model.Title.ToLower().Contains(text.ToLower());
        }

        public override void Selected(UITableView tableView, NSIndexPath path)
        {
            base.Selected(tableView, path);
            _action?.Invoke();
        }
    }
}

