using System;
using CodeFramework.Filters.Models;

namespace CodeHub.Filters.Models
{
    public class PullRequestsFilterModel : FilterModel<PullRequestsFilterModel>
    {
        public bool IsOpen { get; set; }

        public PullRequestsFilterModel()
        {
        }

        public override PullRequestsFilterModel Clone()
        {
            return (PullRequestsFilterModel)this.MemberwiseClone();
        }
    }
}

