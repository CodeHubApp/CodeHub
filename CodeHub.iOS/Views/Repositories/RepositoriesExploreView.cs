using System;
using System.Drawing;
using CodeFramework.iOS.Elements;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels;
using MonoTouch.UIKit;

namespace CodeHub.iOS.Views.Repositories
{
    public sealed class RepositoriesExploreView : ViewModelCollectionDrivenViewController
    {
        public new RepositoriesExploreViewModel ViewModel
        {
            get { return (RepositoriesExploreViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

		public RepositoriesExploreView()
        {
            AutoHideSearch = false;
            //EnableFilter = true;
            NoItemsText = "No Repositories".t();
            Title = "Explore".t();
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
//
//        class ExploreSearchDelegate : UISearchBarDelegate 
//        {
//            readonly RepositoriesExploreView _container;
//
//            public ExploreSearchDelegate (RepositoriesExploreView container)
//            {
//                _container = container;
//            }
//
//            public override void OnEditingStarted (UISearchBar searchBar)
//            {
//                searchBar.ShowsCancelButton = true;
//                _container.ShowSearch(true);
//                _container.NavigationController.SetNavigationBarHidden(true, true);
//            }
//
//            public override void OnEditingStopped (UISearchBar searchBar)
//            {
//                searchBar.ShowsCancelButton = false;
//                _container.FinishSearch ();
//                _container.NavigationController.SetNavigationBarHidden(false, true);
//            }
//
//            public override void TextChanged (UISearchBar searchBar, string searchText)
//            {
//            }
//
//            public override void CancelButtonClicked (UISearchBar searchBar)
//            {
//                searchBar.ShowsCancelButton = false;
//                _container.FinishSearch ();
//                searchBar.ResignFirstResponder ();
//                _container.NavigationController.SetNavigationBarHidden(false, true);
//            }
//
//            public override void SearchButtonClicked (UISearchBar searchBar)
//            {
//                _container.SearchButtonClicked (searchBar.Text);
//                _container.NavigationController.SetNavigationBarHidden(false, true);
//            }
//        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
//            var search = (UISearchBar)TableView.TableHeaderView;
//            search.Delegate = new ExploreSearchDelegate(this);
//
            BindCollection(ViewModel.Repositories, repo =>
            {
                var description = ViewModel.ShouldAlwaysRaiseInpcOnUserInterfaceThread() ? repo.Description : string.Empty;
                var imageUrl = repo.Fork ? Images.GitHubRepoForkUrl : Images.GitHubRepoUrl;
				var sse = new RepositoryElement(repo.Name, repo.StargazersCount, repo.ForksCount, description, repo.Owner.Login, imageUrl) { ShowOwner = true };
                //sse.Tapped += () => NavigationController.PushViewController(new RepositoryViewController(repo.Owner, repo.Name, repo.Name), true);
                return sse;
            });
        }

        public override void SearchButtonClicked(string text)
        {
            View.EndEditing(true);
            ViewModel.SearchCommand.Execute(null);
//
//            try
//            {
//                this.DoWorkTest("Searching...".t(), async () => await ViewModel.Search(text));
//            }
//            catch (Exception e)
//            {
//                MonoTouch.Utilities.ShowAlert("Error".t(), e.Message);
//                MonoTouch.Utilities.LogException(e);
//            }
        }
    }
}

