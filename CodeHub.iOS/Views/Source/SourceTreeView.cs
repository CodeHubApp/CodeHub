using System;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Source;
using GitHubSharp.Models;
using CodeHub.iOS.ViewControllers.Repositories;
using CodeHub.iOS.DialogElements;
using System.Reactive.Linq;

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
            BindCollection(ViewModel.Content, CreateElement);
            ViewModel.Bind(x => x.ShouldShowPro).Where(x => x).Subscribe(_ => this.ShowPrivateView());
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

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            NavigationItem.RightBarButtonItem = null;
        }

        private Element CreateElement(ContentModel x)
        {
            var weakVm = new WeakReference<SourceTreeViewModel>(ViewModel);
            if (x.Type.Equals("dir", StringComparison.OrdinalIgnoreCase))
            {
                var e = new StringElement(x.Name, Octicon.FileDirectory.ToImage());
                e.Clicked.Subscribe(_ => weakVm.Get()?.GoToItemCommand.Execute(x));
                return e;
            }
            if (x.Type.Equals("file", StringComparison.OrdinalIgnoreCase))
            {
                if (x.DownloadUrl != null)
                {
                    var e = new StringElement(x.Name, Octicon.FileCode.ToImage());
                    e.Clicked.Subscribe(_ => weakVm.Get()?.GoToItemCommand.Execute(x));
                    return e;
                }
                else
                {
                    var e = new StringElement(x.Name, Octicon.FileSubmodule.ToImage());
                    e.Clicked.Subscribe(_ => weakVm.Get()?.GoToItemCommand.Execute(x));
                    return e;
                }
            }
            
            return new StringElement(x.Name) { Image = Octicon.FileMedia.ToImage() };
        }
    }
}

