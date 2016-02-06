using System;
using MonoTouch.Dialog;
using UIKit;
using Foundation;
using CodeHub.iOS.TableViewCells;
using GitHubSharp.Models;
using System.Linq;
using CodeHub.Core.Utilities;

namespace CodeHub.iOS.Elements
{
    public class GistElement : Element
    {   
        private readonly Action _action;    
        private readonly GistModel _gist;

        public GistElement(GistModel gist, Action action)
            : base(null)
        {
            _gist = gist;
            _action = action;
        }

        protected override NSString CellKey {
            get {
                return GistCellView.Key;
            }
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            var c = tv.DequeueReusableCell(CellKey) as GistCellView ?? GistCellView.Create();
            var title = _gist.Files?.Select(x => x.Key).FirstOrDefault() ?? "Gist #" + _gist.Id;
            c.Set(title, _gist.Description, _gist.CreatedAt, new GitHubAvatar(_gist.Owner?.AvatarUrl));
            return c;
        }

        public override bool Matches(string text)
        {
            return _gist.Description.ToLower().Contains(text.ToLower());
        }

        public override void Selected(DialogViewController dvc, UITableView tableView, NSIndexPath path)
        {
            _action?.Invoke();
            tableView.DeselectRow (path, true);
        }
    }
}

