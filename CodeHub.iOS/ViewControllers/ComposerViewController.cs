using CodeHub.iOS.ViewControllers.App;
using ReactiveUI;
using UIKit;
using System;
using CodeHub.Core.Services;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels;

namespace CodeHub.iOS.ViewControllers
{
    public class ComposerViewController : MarkdownComposerViewController<ComposerViewModel>, IModalViewController
    {
        public ComposerViewController(IMarkdownService markdownService) 
            : base(markdownService)
        {
            this.WhenAnyValue(x => x.ViewModel.SaveCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Save))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);

            this.WhenAnyValue(x => x.ViewModel.DismissCommand)
                .Select(x => x.ToBarButtonItem(Images.Cancel))
                .Subscribe(x => NavigationItem.LeftBarButtonItem = x);
        }
    }
}

