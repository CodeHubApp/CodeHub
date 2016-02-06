using System;
using MonoTouch.Dialog;
using GitHubSharp.Models;
using Foundation;
using UIKit;
using CodeHub.Core.Utilities;
using CodeHub.iOS.TableViewCells;
using Humanizer;

namespace CodeHub.iOS.Elements
{
    public class CommitElement : Element
    {   
        private readonly Action _action;    
        private readonly CommitModel _model;

        public CommitElement(CommitModel model, Action action)
            : base(null)
        {
            _model = model;
            _action = action;
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            var c = tv.DequeueReusableCell(CommitCellView.Key) as CommitCellView ?? CommitCellView.Create();
            var msg = _model?.Commit?.Message ?? string.Empty;
            var firstLine = msg.IndexOf("\n", StringComparison.Ordinal);
            var description = firstLine > 0 ? msg.Substring(0, firstLine) : msg;
            var time = DateTimeOffset.MinValue;
            if (_model.Commit.Committer != null)
                time = _model.Commit.Committer.Date;

            var name = _model.GenerateCommiterName();
            var avatar = new GitHubAvatar(_model.GenerateGravatarUrl());

            c.Set(name, description, time, avatar);
            return c;
        }

        public override bool Matches(string text)
        {
            return _model.Commit.Message.ToLower().Contains(text.ToLower());
        }

        public override void Selected(DialogViewController dvc, UITableView tableView, NSIndexPath path)
        {
            _action?.Invoke();
            tableView.DeselectRow (path, true);
        }
    }
}



