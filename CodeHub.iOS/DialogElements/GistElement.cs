using System.Linq;
using CodeHub.Core.Utilities;
using CodeHub.iOS.TableViewCells;
using Humanizer;
using Octokit;
using UIKit;

namespace CodeHub.iOS.DialogElements
{
    public class GistElement : Element
    {
        private GitHubAvatar Avatar { get; }
        private string Title { get; }
        private string Description { get; }
        private string UpdatedString { get; }

        private static string GetGistTitle(Gist gist)
        {
            var title = (gist.Owner == null) ? "Anonymous" : gist.Owner.Login;
            if (gist.Files.Count > 0)
                title = gist.Files.First().Key;
            return title;
        }

        public GistElement(Gist gist)
        {
            Title = GetGistTitle(gist);
            Description = string.IsNullOrEmpty(gist.Description) ? "Gist " + gist.Id : gist.Description;
            Avatar = new GitHubAvatar(gist.Owner?.AvatarUrl);
            UpdatedString = gist.UpdatedAt.Humanize();
        }

        public override UITableViewCell GetCell(UITableView tv)
        {
            var c = tv.DequeueReusableCell(GistCellView.Key) as GistCellView ?? GistCellView.Create();
            c.Set(Title, Description, UpdatedString, Avatar);
            return c;
        }

        public override bool Matches(string text)
        {
            var lowerText = text.ToLower();
            return new[] { Description, Title }.Any(x => x.Contains(lowerText));
        }
    }
}
