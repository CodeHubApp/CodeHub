using System;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using System.Collections.Generic;
using CodeHub.Controllers;
using System.Linq;
using MonoTouch;
using CodeFramework.Controllers;
using CodeFramework.Elements;
using CodeHub.GitHub.Controllers.Changesets;

namespace CodeHub.GitHub.Controllers.Changesets
{
    public class ChangesetController : BaseModelDrivenController
    {
        private string _lastNode;
        private LoadMoreElement _loadMore;

        public string User { get; private set; }
        public string Slug { get; private set; }

        public new List<CommitModel> Model { get { return (List<CommitModel>)base.Model; } }

        public ChangesetController(string user, string slug)
            : base(typeof(List<CommitModel>))
        {
            User = user;
            Slug = slug;
            Style = MonoTouch.UIKit.UITableViewStyle.Plain;
            Title = "Changes";
        }

        protected virtual List<CommitModel> OnGetData(string startNode = null)
        {
            return Application.Client.API.GetCommits(User, Slug, startNode).Data;
        }

        private void GetMore()
        {
            this.DoWorkNoHud(() => {
                var newChanges = OnGetData(_lastNode);

                //Always remove the first node since it should already be listed...
                if (newChanges.Count > 0)
                    newChanges.RemoveAt(0);

                //Save the last node...
                if (newChanges.Count > 0)
                {
                    InvokeOnMainThread(() => AddItems(Root, newChanges));
                    _lastNode = newChanges.Last().Sha;
                }

                //Should never happen. Sanity check..
                if (_loadMore != null && newChanges.Count == 0)
                {
                    InvokeOnMainThread(() => {
                        Root.Remove(_loadMore.Parent as Section);
                        _loadMore.Dispose();
                        _loadMore = null;
                    });
                }
            }, 
            ex => Utilities.ShowAlert("Failure to load!", "Unable to load additional enries! " + ex.Message),
            () => {
                if (_loadMore != null)
                    _loadMore.Animating = false;
            });
        }

        private void AddItems(RootElement root, List<CommitModel> changes)
        {
            var sec = new Section();
            changes.ForEach(x => {
                var desc = (x.Commit.Message ?? "").Replace("\n", " ").Trim();
                var el = new NameTimeStringElement { Name = x.Author.Login, Time = (x.Commit.Author.Date.ToDaysAgo()), String = desc, Lines = 4 };
                el.Tapped += () => NavigationController.PushViewController(new ChangesetInfoController(User, Slug, x.Sha), true);
                sec.Add(el);
            });

            if (sec.Count > 0)
            {
                root.Insert(root.Count - 1, sec);
            }
        }

        protected override void OnRender()
        {
            //Create some needed elements
            var root = new RootElement(Title) { UnevenRows = true };
            _loadMore = new PaginateElement("Load More", "Loading...", e => GetMore());
            root.Add(new Section { _loadMore });

            //Add the items that were in the update
            AddItems(root, Model);

            //Update the UI
            Root = root;
        }

        protected override object OnUpdateModel(bool forced)
        {
            var changes = OnGetData();
            _lastNode = changes.Last().Sha;
            return changes;
        }
    }
}

