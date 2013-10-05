using System;
using GitHubSharp.Models;
using System.Collections.Generic;
using CodeFramework.Controllers;

namespace CodeHub.Controllers
{
    public class IssueInfoController : Controller<IssueInfoController.ViewModel>
    {
        public long Id { get; private set; }
        public string User { get; private set; }
        public string Slug { get; private set; }
        public Action<IssueModel> ModelChanged;

        public IssueInfoController(IView<ViewModel> view, string user, string slug, long id)
            : base(view)
        {
            User = user;
            Slug = slug;
            Id = id;
        }

        protected override void OnUpdate(bool forceDataRefresh)
        {
            var l = Application.Client.Users[User].Repositories[Slug].Issues[Id];
            var comments = Application.Client.Execute(l.GetComments());
            Model = new IssueInfoController.ViewModel {
                Comments = comments.Data,
                MoreComments = HandleMoreComments(comments.More),
                Issue = Application.Client.Execute(l.Get()).Data,
            };
        }

        //Custome more handler
        private Action HandleMoreComments(GitHubSharp.GitHubRequest<List<IssueCommentModel>> request)
        {
            if (request == null)
                return null;

            return () => {
                var data = Application.Client.Execute(request);
                var items = data.Data;
                Model.Comments.AddRange(items);
                Model.MoreComments = HandleMoreComments(data.More);
                RenderView();
            };
        }

        public void EditComplete(IssueModel model)
        {
            Model.Issue = model;
            if (ModelChanged != null)
                ModelChanged(model);

            //If model == null then it has been deleted. No need to render.
            if (model != null)
                RenderView();
        }

        public void AddComment(string text)
        {
            var comment = Application.Client.Execute(Application.Client.Users[User].Repositories[Slug].Issues[Id].CreateComment(text));
            Model.Comments.Add(comment.Data);
            RenderView();
        }

        public class ViewModel
        {
            public IssueModel Issue { get; set; }
            public List<IssueCommentModel> Comments { get; set; }
            public Action MoreComments;
        }
    }
}

