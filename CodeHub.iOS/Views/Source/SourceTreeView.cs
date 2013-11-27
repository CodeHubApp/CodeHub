using System;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels.Source;
using CodeHub.ViewControllers;
using GitHubSharp.Models;
using MonoTouch.Dialog;

namespace CodeHub.iOS.Views.Source
{
    public class SourceTreeView : ViewModelCollectionDrivenViewController
    {
        public new SourceTreeViewModel ViewModel
        {
            get { return (SourceTreeViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public override void ViewDidLoad()
        {
            EnableFilter = true;

            base.ViewDidLoad();

            BindCollection(ViewModel.Content, CreateElement);
        }

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			Title = string.IsNullOrEmpty(ViewModel.Path) ? ViewModel.Repository : ViewModel.Path.Substring(ViewModel.Path.LastIndexOf('/') + 1);
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

//
//        protected override CodeFramework.Filters.ViewControllers.FilterViewController CreateFilterController()
//        {
//            return new CodeHub.Filters.ViewControllers.SourceFilterViewController(ViewModel.Content);
//        }
    }
}

