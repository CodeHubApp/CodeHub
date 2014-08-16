using System;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssueItemViewModel
    {
        public IssueModel Issue { get; set; }

        public string RepositoryName { get; set; }

        public string RepositoryFullName { get; set; }

        public string RepositoryOwner { get; set; }

        public bool IsPullRequest { get; set; }
    }
}

