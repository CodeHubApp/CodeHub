using System;
using CodeHub.Core.ViewModels.Changesets;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.iOS.Views.App;
using CodeHub.Core.Services;

namespace CodeHub.iOS.Views.Source
{
    public class CommitCommentView : MarkdownComposerView<CommitCommentViewModel>
    {
        public CommitCommentView(IMarkdownService markdownService) 
            : base(markdownService)
        {
            this.WhenAnyValue(x => x.ViewModel.SaveCommand).Subscribe(x =>
                NavigationItem.RightBarButtonItem = x != null ? x.ToBarButtonItem(UIBarButtonSystemItem.Save) : null);
        }
    }
}

