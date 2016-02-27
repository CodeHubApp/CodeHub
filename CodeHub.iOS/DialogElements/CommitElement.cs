using System;
using GitHubSharp.Models;
using Foundation;
using UIKit;
using CodeHub.Core.Utilities;
using CodeHub.iOS.TableViewCells;

namespace CodeHub.iOS.DialogElements
{
    public class CommitElement : Element
    {   
        private readonly Action _action;    
        private readonly CommitModel _model;
        private readonly GitHubAvatar _avatar;
        private readonly string _name;
        private readonly string _description;

        public CommitElement(CommitModel model, Action action)
        {
            _model = model;
            _action = action;
            _avatar = new GitHubAvatar(_model.GenerateGravatarUrl());
            _name = _model.GenerateCommiterName();

            var msg = _model?.Commit?.Message ?? string.Empty;
            var firstLine = msg.IndexOf("\n", StringComparison.Ordinal);
            _description = firstLine > 0 ? msg.Substring(0, firstLine) : msg;
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            var c = tv.DequeueReusableCell(CommitCellView.Key) as CommitCellView ?? CommitCellView.Create();
            var time = DateTimeOffset.MinValue;
            if (_model.Commit.Committer != null)
                time = _model.Commit.Committer.Date;
            c.Set(_name, _description, time, _avatar);
            return c;
        }

        public override bool Matches(string text)
        {
            return _model?.Commit?.Message?.ToLower().Contains(text.ToLower()) ?? false;
        }

        public override void Selected(UITableView tableView, NSIndexPath path)
        {
            base.Selected(tableView, path);
            _action?.Invoke();
        }
    }
}



