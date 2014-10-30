using GitHubSharp.Models;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueItemViewModel : ReactiveObject
    {
        public IssueModel Issue { get; set; }

        public string RepositoryName { get; set; }

        public string RepositoryFullName { get; set; }

        public string RepositoryOwner { get; set; }

        public bool IsPullRequest { get; set; }

        public IReactiveCommand GoToCommand { get; private set; }
    }
}

