using System;
using GitHubSharp.Models;
using System.Linq;
using CodeFramework.Controllers;
using System.Collections.Generic;

namespace CodeHub.Controllers
{
    public class ChangesetInfoController : Controller<ChangesetInfoController.ViewModel>
    {
        public string Node { get; private set; }

        public string User { get; private set; }

        public string Slug { get; private set; }

        public RepositoryModel Repo { get; set; }

        public ChangesetInfoController(IView<ViewModel> view, string user, string slug, string node)
            : base(view)
        {
            Node = node;
            User = user;
            Slug = slug;
        }

        protected override void OnUpdate(bool forceDataRefresh)
        {
            Model = new ViewModel();

            this.RequestModel(Application.Client.Users[User].Repositories[Slug].Commits[Node].Get(), forceDataRefresh, response => {
                var data = response.Data;
                data.Files = data.Files.OrderBy(y => y.Filename.Substring(y.Filename.LastIndexOf('/') + 1)).ToList();
                Model.Changeset = data;
                RenderView();
            });


            this.RequestModel(Application.Client.Users[User].Repositories[Slug].Commits[Node].Comments.GetAll(), forceDataRefresh, response => {
                Model.Comments = response.Data;
                RenderView();
            });
        }
 
        public void AddComment(string text)
        {
            var c = Application.Client.Execute(Application.Client.Users[User].Repositories[Slug].Commits[Node].Comments.Create(text));
            Model.Comments.Add(c.Data);
            RenderView();
        }

        /// <summary>
        /// An inner class that combines two external models
        /// </summary>
        public class ViewModel
        {
            public CommitModel Changeset { get; set; }
            public List<CommentModel> Comments { get; set; }

            public ViewModel()
            {
                Comments = new List<CommentModel>();
            }
        }
    }
}

