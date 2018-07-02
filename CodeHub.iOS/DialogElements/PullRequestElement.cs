using System;
using Foundation;
using CodeHub.iOS.Cells;
using UIKit;
using CodeHub.Core.Utilities;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace CodeHub.iOS.DialogElements
{
    public class PullRequestElement : Element
    {   
        private readonly Subject<PullRequestElement> _tapped = new Subject<PullRequestElement>();
        private readonly Octokit.PullRequest _model;

        public IObservable<object> Clicked => _tapped.AsObservable();

        public PullRequestElement(Octokit.PullRequest model)
        {
            _model = model;
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
            _tapped.OnNext(this);
        }
    }
}

