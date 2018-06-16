using System;
using Foundation;
using UIKit;
using CodeHub.Core.Utilities;
using CodeHub.iOS.TableViewCells;

namespace CodeHub.iOS.DialogElements
{
    public class CommitElement : Element
    {   
        private readonly Action _action;    
        private readonly GitHubAvatar _avatar;
        private readonly string _name;
        private readonly string _description;
        private readonly DateTimeOffset _date;

        public CommitElement(
            string name,
            Uri avatarUrl,
            string message,
            DateTimeOffset? date,
            Action action)
        {
            _action = action;
            _avatar = new GitHubAvatar(avatarUrl);
            _name = name;
            _date = date ?? DateTimeOffset.MinValue;

            var msg = message ?? string.Empty;
            var firstLine = msg.IndexOf("\n", StringComparison.Ordinal);
            _description = firstLine > 0 ? msg.Substring(0, firstLine) : msg;
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            var c = tv.DequeueReusableCell(CommitCellView.Key) as CommitCellView ?? CommitCellView.Create();
            c.Set(_name, _description, _date, _avatar);
            return c;
        }

        public override bool Matches(string text)
        {
            return _description.ToLower().Contains(text.ToLower());
        }

        public override void Selected(UITableView tableView, NSIndexPath path)
        {
            base.Selected(tableView, path);
            _action?.Invoke();
        }
    }
}



