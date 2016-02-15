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

        public GistElement(GistModel gist, Action action)
        {
            _gist = gist;
            _action = action;
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            var c = tv.DequeueReusableCell(GistCellView.Key) as GistCellView ?? GistCellView.Create();
            var title = _gist.Files?.Select(x => x.Key).FirstOrDefault() ?? "Gist #" + _gist.Id;
            c.Set(title, _gist.Description, _gist.CreatedAt, new GitHubAvatar(_gist.Owner?.AvatarUrl));
            return c;
        }

        public override bool Matches(string text)
        {
            return (_gist.Description ?? string.Empty).ToLower().Contains((text ?? string.Empty).ToLower());
        }

        public override void Selected(UITableView tableView, NSIndexPath path)
        {
            base.Selected(tableView, path);
            _action?.Invoke();
        }
    }
}

