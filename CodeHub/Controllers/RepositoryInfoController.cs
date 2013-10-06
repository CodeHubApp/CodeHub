using CodeFramework.Controllers;
using GitHubSharp.Models;

namespace CodeHub.Controllers
{
    public class RepositoryInfoController : Controller<RepositoryInfoController.ViewModel>
    {
        public string User { get; private set; }

        public string Repo { get; private set; }

        public RepositoryInfoController(IView<ViewModel> view, string user, string repo)
            : base(view)
        {
            User = user;
            Repo = repo;
        }

        protected override void OnUpdate(bool forceDataRefresh)
        {
            Model = new ViewModel();

            this.RequestModel(Application.Client.Users[User].Repositories[Repo].Get(), forceDataRefresh, response => {
                Model.RepositoryModel = response.Data;
                RenderView();
            });

            this.RequestModel(Application.Client.Users[User].Repositories[Repo].IsWatching(), forceDataRefresh, response => {
                Model.IsWatched = response.Data;
            });

            this.RequestModel(Application.Client.Users[User].Repositories[Repo].IsStarred(), forceDataRefresh, response => {
                Model.IsStarred = response.Data;
            });

            this.RequestModel(Application.Client.Users[User].Repositories[Repo].GetReadme(), forceDataRefresh, response => {
                Model.Readme = response.Data;
                RenderView();
            });
        }

        public void Watch()
        {
            Application.Client.Execute(Application.Client.Users[User].Repositories[Repo].Watch());
            Model.IsWatched = true;
        }

        public void StopWatching()
        {
            Application.Client.Execute(Application.Client.Users[User].Repositories[Repo].StopWatching());
            Model.IsWatched = false;
        }

        public void Star()
        {
            Application.Client.Execute(Application.Client.Users[User].Repositories[Repo].Star());
            Model.IsStarred = true;
        }

        public void Unstar()
        {
            Application.Client.Execute(Application.Client.Users[User].Repositories[Repo].Unstar());
            Model.IsStarred = false;
        }

        public class ViewModel
        {
            public RepositoryModel RepositoryModel;
            public bool IsWatched;
            public bool IsStarred;
            public ContentModel Readme;
        }
    }
}

