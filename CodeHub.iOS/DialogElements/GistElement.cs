using System;
using UIKit;
using Foundation;
using CodeHub.iOS.TableViewCells;
using GitHubSharp.Models;
using System.Linq;
using CodeHub.Core.Utilities;

namespace CodeHub.iOS.DialogElements
{
    public class GistElement : Element
    {   
        private readonly Action _action;    
        private readonly GistModel _gist;
        private readonly string _title;

        public GistElement(GistModel gist, Action action)
        {
            _gist = gist;
            _action = action;
            _title = _gist.Files?.Select(x => x.Key).FirstOrDefault() ?? "Gist #" + _gist.Id;
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            var c = tv.DequeueReusableCell(GistCellView.Key) as GistCellView ?? GistCellView.Create();
            c.Set(_title, _gist.Description, _gist.CreatedAt, new GitHubAvatar(_gist.Owner?.AvatarUrl));
            return c;
        }

        public override bool Matches(string text)
        {
            return _title.ContainsKeyword(text) || _gist.Description.ContainsKeyword(text);
        }

        public override void Selected(UITableView tableView, NSIndexPath path)
        {
            base.Selected(tableView, path);
            _action?.Invoke();
        }
    }
}

