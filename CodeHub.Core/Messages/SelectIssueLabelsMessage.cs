using System.Collections.Generic;
using System.Linq;

namespace CodeHub.Core.Messages
{
    public class SelectIssueLabelsMessage
    {
        public Octokit.Label[] Labels { get; }

        public SelectIssueLabelsMessage(IEnumerable<Octokit.Label> labels)
        {
            Labels = labels.ToArray();
        }
    }
}

