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

        public override void Update(bool force)
        {
            var model = new ViewModel();
            var x = Application.Client.Users[User].Repositories[Slug].Commits[Node].Get(force).Data;
            x.Files = x.Files.OrderBy(y => y.Filename.Substring(y.Filename.LastIndexOf('/') + 1)).ToList();
            model.Changeset = x;

            try
            {
                model.Comments = Application.Client.Users[User].Repositories[Slug].Commits[Node].Comments.GetAll(force).Data;
            }
            catch (Exception e)
            {
                MonoTouch.Utilities.LogException("Unable to get comments", e);
            }

            Model = model;
        }


 
        public void AddComment(string text)
        {
            var c = Application.Client.Users[User].Repositories[Slug].Comments.Create(text);
            Model.Comments.Add(c.Data);
            Render();
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

