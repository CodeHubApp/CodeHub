using CodeHub.iOS.Views.App;
using ReactiveUI;
using UIKit;
using System;
using CodeHub.Core.Services;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels;

namespace CodeHub.iOS.Views
{
    public class ComposerView : MarkdownComposerView<ComposerViewModel>, IModalView
    {
        public ComposerView(IMarkdownService markdownService) 
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

