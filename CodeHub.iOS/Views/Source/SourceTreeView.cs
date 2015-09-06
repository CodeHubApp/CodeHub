using System;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels.Source;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using CodeHub.iOS.Views.Filters;

namespace CodeHub.iOS.Views.Source
{
    public class SourceTreeView : ViewModelCollectionDrivenDialogViewController
    {
        public new SourceTreeViewModel ViewModel
        {
            get { return (SourceTreeViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.RightBarButtonItem = new UIKit.UIBarButtonItem(Theme.CurrentTheme.SortButton, UIKit.UIBarButtonItemStyle.Plain, 
                (s, e) => ShowFilterController(new SourceFilterViewController(ViewModel.Content))); 
            BindCollection(ViewModel.Content, CreateElement);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (string.IsNullOrEmpty(ViewModel.Path))
                Title = ViewModel.Repository;
            else
            {
                var path = ViewModel.Path.TrimEnd('/');
                Title = path.Substring(path.LastIndexOf('/') + 1);
            } 
        }

        private Element CreateElement(ContentModel x)
        {
            if (x.Type.Equals("dir", StringComparison.OrdinalIgnoreCase))
            {
                return new StyledStringElement(x.Name, () => ViewModel.GoToSourceTreeCommand.Execute(x), Images.Folder);
            }
            if (x.Type.Equals("file", StringComparison.OrdinalIgnoreCase))
            {
                //If there's a size, it's a file
                if (x.Size != null)
                {
                    return new StyledStringElement(x.Name, () => ViewModel.GoToSourceCommand.Execute(x), Images.File);
                }

                //If there is no size, it's most likey a submodule
                return new StyledStringElement(x.Name, () => ViewModel.GoToSubmoduleCommand.Execute(x), Images.Repo);
            }
            
            return new StyledStringElement(x.Name) { Image = Images.File };
        }
    }
}

