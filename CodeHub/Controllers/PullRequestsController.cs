using CodeFramework.Controllers;
using GitHubSharp.Models;
using CodeHub.Filters.Models;

namespace CodeHub.Controllers
{
    public class PullRequestsController : ListController<PullRequestModel, PullRequestsFilterModel>
    {
        public string User { get; private set; }

        public string Repo { get; private set; }

        public PullRequestsController(IListView<PullRequestModel> view, string user, string repo) 
            : base(view)
        {
            User = user;
            Repo = repo;

            Filter = Application.Account.GetFilter<PullRequestsFilterModel>(this);
        }

        protected override void OnUpdate(bool forceDataRefresh)
        {
            var state = Filter.IsOpen ? "open" : "closed";
            var request = Application.Client.Users[User].Repositories[Repo].PullRequests.GetAll(state: state);
            this.RequestModel(request, forceDataRefresh, response => {
                RenderView(new ListModel<PullRequestModel>(response.Data, this.CreateMore(response)));
            });
        }

        protected override void SaveFilterAsDefault(PullRequestsFilterModel filter)
        {
            Application.Account.AddFilter(this, filter);
        }
    }
}
