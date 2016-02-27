using UIKit;
using CodeHub.iOS.TableViewCells;
using System;
using CodeHub.Core.Utilities;

namespace CodeHub.iOS.DialogElements
{
    public class CommentElement : Element
    {   
        private readonly string _title, _message, _avatar;
        private readonly DateTimeOffset _date;

        public CommentElement(string title, string message, DateTimeOffset date, string avatar)
        {
            _title = title;
            _message = message;
            _date = date;
            _avatar = avatar;
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            var c = tv.DequeueReusableCell(CommitCellView.Key) as CommitCellView ?? CommitCellView.Create();
            c.Set(_title, _message, _date, new GitHubAvatar(_avatar));
            return c;
        }

        public override bool Matches(string text)
        {
            return _message?.ToLower().Contains(text.ToLower()) ?? false;
        }
    }
}

