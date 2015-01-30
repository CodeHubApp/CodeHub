using System;
using CodeHub.Core.ViewModels.Changesets;
using UIKit;
using ReactiveUI;
using CodeHub.iOS.Views.App;
using CodeHub.Core.Services;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Source
{
    public class CommitCommentView : MarkdownComposerView<CommitCommentViewModel>
    {
        public CommitCommentView(IMarkdownService markdownService) 
            : base(markdownService)
        {
            this.WhenAnyValue(x => x.ViewModel.SaveCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Save))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);
        }
    }
}

