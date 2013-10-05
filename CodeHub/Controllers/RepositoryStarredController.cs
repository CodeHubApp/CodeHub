using GitHubSharp.Models;
using CodeFramework.Controllers;

namespace CodeHub.Controllers
{
    public class RepositoryStarredController : ListController<BasicUserModel>
    {
        private readonly string _name;
        private readonly string _owner;
        
        public RepositoryStarredController(IListView<BasicUserModel> view, string owner, string name)
            : base(view)
        {
            _name = name;
            _owner = owner;
        }

        protected override void OnUpdate(bool forceDataRefresh)
        {
            this.RequestModel(Application.Client.Users[_owner].Repositories[_name].GetStargazers(), forceDataRefresh, response => {
                RenderView(new ListModel<BasicUserModel>(response.Data, this.CreateMore(response)));
            });
        }
    }
}

