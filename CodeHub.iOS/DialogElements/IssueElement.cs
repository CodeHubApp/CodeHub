using System;
using UIKit;
using Foundation;
using CodeHub.iOS.TableViewCells;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace CodeHub.iOS.DialogElements
{
    public class IssueElement : Element, IElementSizing
    {
        private readonly Subject<IssueElement> _tapped = new Subject<IssueElement>();
        private readonly Octokit.Issue _model;

        public IObservable<IssueElement> Clicked => _tapped.AsObservable();

        public IssueElement(Octokit.Issue model) 
        {
            _model = model;
        }

        public nfloat GetHeight (UITableView tableView, NSIndexPath indexPath)
        {
            return 69f;
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            var cell = tv.DequeueReusableCell(IssueCellView.Key) as IssueCellView ?? IssueCellView.Create();
            var assigned = _model.Assignee?.Login ?? "unassigned";
            var commentString = _model.Comments == 1 ? "1 comment" : _model.Comments + " comments";
            var updatedAt = _model.UpdatedAt ?? _model.CreatedAt;
            cell.Bind(_model.Title, _model.State.StringValue, commentString, assigned, updatedAt, _model.Number.ToString(), "Issue");
            return cell;
        }

        public override void Selected(UITableView tableView, NSIndexPath path)
        {
            base.Selected(tableView, path);
            _tapped.OnNext(this);
        }

        public override bool Matches(string text)
        {
            var id = _model.Number.ToString();
            var title = _model.Title ?? string.Empty;
            return id.IndexOf(text, StringComparison.OrdinalIgnoreCase) != -1 || title.IndexOf(text, StringComparison.OrdinalIgnoreCase) != -1;
        }
    }
}

