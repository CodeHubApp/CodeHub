using CodeHub.Core.Utilities;
using CodeHub.iOS.TableViewCells;
using Octokit;
using UIKit;

namespace CodeHub.iOS.DialogElements
{
    public class RepositoryElement : Element
    {
        private string Owner => Repository.Owner?.Login ?? string.Empty;
        private GitHubAvatar Avatar => new GitHubAvatar(Repository.Owner?.AvatarUrl);
        private string Description { get; }
        private bool ShowOwner { get; }
        private Repository Repository { get; }

        public RepositoryElement(Repository repository, bool showOwner = true, bool showDescription = true)
        {
            if (showDescription)
            {
                if (!string.IsNullOrEmpty(repository.Description) && repository.Description.IndexOf(':') >= 0)
                    Description = Emojis.FindAndReplace(repository.Description);
                else
                    Description = repository.Description;
            }
            else
            {
                Description = null;
            }

            Repository = repository;
            ShowOwner = showOwner;
        }

        public override UITableViewCell GetCell(UITableView tv)
        {
            var c = tv.DequeueReusableCell(RepositoryCellView.Key) as RepositoryCellView ?? RepositoryCellView.Create();
            c.Set(Repository.Name, Repository.StargazersCount, Repository.ForksCount, Description, Owner, Avatar, ShowOwner);
            return c;
        }

        public override bool Matches(string text)
        {
            var lowerText = text.ToLower();
            return Repository.Name.Contains(lowerText);
        }
    }
}
