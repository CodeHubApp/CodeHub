using System;
using GitHubSharp.Models;
using System.Collections.Generic;
using CodeFramework.Controllers;

namespace CodeHub.Controllers
{
    public class IssueInfoController : Controller<IssueInfoController.ViewModel>
    {
        public int Id { get; private set; }
        public string User { get; private set; }
        public string Slug { get; private set; }
        public Action<IssueModel> ModelChanged;

        public IssueInfoController(IView<ViewModel> view, string user, string slug, int id)
            : base(view)
        {
            User = user;
            Slug = slug;
            Id = id;
        }

        public override void Update(bool force)
        {
//            var l = Application.Client.Users[User].Repositories[Slug].Issues[Id];
//            Model = new IssueInfoController.IssueInfoModel {
//                Comments = l.Comments.GetComments(force),
//                Issue = l.GetIssue(force),
//            };
        }

        public void EditComplete(IssueModel model)
        {
//            Model.Issue = model;
//            if (ModelChanged != null)
//                ModelChanged(model);
//
//            //If model == null then it has been deleted. No need to render.
//            if (model != null)
//                Render();
        }

        public void AddComment(string text)
        {
//            var comment = Application.Client.Users[User].Repositories[Slug].Issues[Id].Comments.Create(text);
//            Model.Comments.Add(comment);
//            Render();
        }

        public class ViewModel
        {
            public IssueModel Issue { get; set; }
            public List<CommentModel> Comments { get; set; }
        }
    }
}

