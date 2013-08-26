using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using System.Threading;
using System.Linq;
using RedPlum;
using System.Drawing;
using MonoTouch;
using System.Collections.Generic;
using GitHubSharp.Models;
using CodeHub.Controllers;
using CodeFramework.Controllers;
using CodeFramework.Elements;
using CodeFramework.Filters.Controllers;

namespace CodeHub.ViewControllers
{
    public sealed class RepositoriesExploreViewController : BaseListControllerDrivenViewController, IListView<RepositorySearchModel.RepositoryModel>
    {
        public new RepositoriesExploreController Controller
        {
            get { return (RepositoriesExploreController)base.Controller; }
            private set { base.Controller = value; }
        }

		public RepositoriesExploreViewController()
        {
            AutoHideSearch = false;
            EnableFilter = true;
            SearchPlaceholder = "Search Repositories".t();
            NoItemsText = "No Repositories".t();
            Title = "Explore".t();
            Controller = new RepositoriesExploreController(this);
        }

        public void Render(ListModel<RepositorySearchModel.RepositoryModel> model)
        {
            if (!Controller.Searched)
                return;

            RenderList(model, repo => {
                var description = Application.Account.HideRepositoryDescriptionInList ? string.Empty : repo.Description;
                var sse = new RepositoryElement(repo.Name, repo.Watchers, repo.Forks, description, repo.Owner, null) { ShowOwner = true };
                sse.Tapped += () => NavigationController.PushViewController(new RepositoryInfoViewController(repo.Owner, repo.Name, repo.Name), true);
                return sse;
            });
        }

        void ShowSearch(bool value)
        {
            if (!value)
            {
                if (TableView.ContentOffset.Y < 44)
                    TableView.ContentOffset = new PointF (0, 44);
            }
            else
            {
                TableView.ContentOffset = new PointF (0, 0);
            }
        }

        class ExploreSearchDelegate : UISearchBarDelegate 
        {
            readonly RepositoriesExploreViewController _container;

            public ExploreSearchDelegate (RepositoriesExploreViewController container)
            {
                _container = container;
            }

            public override void OnEditingStarted (UISearchBar searchBar)
            {
                searchBar.ShowsCancelButton = true;
                _container.SearchStart ();
                _container.ShowSearch(true);
                _container.NavigationController.SetNavigationBarHidden(true, true);
            }

            public override void OnEditingStopped (UISearchBar searchBar)
            {
                searchBar.ShowsCancelButton = false;
                _container.FinishSearch ();
                _container.NavigationController.SetNavigationBarHidden(false, true);
                _container.SearchEnd();
            }

            public override void TextChanged (UISearchBar searchBar, string searchText)
            {
            }

            public override void CancelButtonClicked (UISearchBar searchBar)
            {
                searchBar.ShowsCancelButton = false;
                _container.FinishSearch ();
                searchBar.ResignFirstResponder ();
                _container.NavigationController.SetNavigationBarHidden(false, true);
                _container.SearchEnd();
            }

            public override void SearchButtonClicked (UISearchBar searchBar)
            {
                _container.SearchButtonClicked (searchBar.Text);
                _container.NavigationController.SetNavigationBarHidden(false, true);
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            var search = (UISearchBar)TableView.TableHeaderView;
            search.Delegate = new ExploreSearchDelegate(this);
        }

        public override void SearchButtonClicked(string text)
        {
            View.EndEditing(true);
            this.DoWork(() => Controller.Search(text));
        }
    }
}

