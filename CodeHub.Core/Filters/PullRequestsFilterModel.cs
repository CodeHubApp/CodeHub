using CodeFramework.Core.ViewModels;

namespace CodeHub.Core.Filters
{
    public class PullRequestsFilterModel : FilterModel<PullRequestsFilterModel>
    {
        public bool IsOpen { get; set; }

        public PullRequestsFilterModel()
        {
            IsOpen = true;
        }

        public override PullRequestsFilterModel Clone()
        {
            return (PullRequestsFilterModel)this.MemberwiseClone();
        }
    }
}

