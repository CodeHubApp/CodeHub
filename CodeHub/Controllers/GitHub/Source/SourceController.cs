using System;
using System.Collections.Generic;
using System.Linq;
using CodeHub.Data;
using CodeHub.Controllers;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using CodeFramework.Controllers;
using CodeFramework.Elements;

namespace CodeHub.GitHub.Controllers.Source
{
    public class SourceController : BaseListModelController
    {
        public string Username { get; private set; }
        public string Slug { get; private set; }
        public string Branch { get; private set; }
        public string Path { get; private set; }

        public SourceController(string username, string slug, string branch = "master", string path = "")
            : base(typeof(List<ContentModel>))
        {
            Username = username;
            Slug = slug;
            Branch = branch;
            Path = path;
            SearchPlaceholder = "Search Files & Folders";
            Title = string.IsNullOrEmpty(path) ? "Source" : path.Substring(path.LastIndexOf('/') + 1);
        }

        protected override Element CreateElement(object obj)
        {
            var contentModel = (ContentModel)obj;
            if (contentModel.Type.Equals("dir", StringComparison.OrdinalIgnoreCase))
            {
                return new StyledStringElement(contentModel.Name, () => NavigationController.PushViewController(
                    new SourceController(Username, Slug, Branch, contentModel.Path), true), Images.Folder);
            }
            else if (contentModel.Type.Equals("file", StringComparison.OrdinalIgnoreCase))
            {
                return new StyledStringElement(contentModel.Name, () => NavigationController.PushViewController(
                    new SourceInfoController(Username, Slug, Branch, contentModel.Path) { Title = contentModel.Name }, true), Images.File);
            }
            else
            {
                return new StyledStringElement(contentModel.Name) { Image = Images.File };
            }
        }

        protected override object OnUpdateListModel(bool forced, int currentPage, ref int nextPage)
        {
            var d = Application.Client.API.GetRepositoryContent(Username, Slug, Path, Branch);
            return d.Data.OrderBy(x => x.Name).GroupBy(x => x.Type).OrderBy(a => a.Key).SelectMany(a => a).ToList();
        }
    }
}


