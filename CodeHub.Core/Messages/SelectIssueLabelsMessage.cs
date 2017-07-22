using System.Collections.Generic;
using System.Linq;
using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
    public class SelectIssueLabelsMessage
    {
        public LabelModel[] Labels { get; }

        public SelectIssueLabelsMessage(IEnumerable<LabelModel> labels)
        {
            Labels = labels.ToArray();
        }
    }
}

