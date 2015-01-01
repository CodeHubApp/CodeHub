using System;
using CodeHub.iOS.Views.App;
using ReactiveUI;
using CodeHub.Core.ViewModels.Issues;
using MonoTouch.UIKit;
using CodeHub.Core.Services;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueCommentView : MarkdownComposerView<IssueCommentViewModel>
    {
        public IssueCommentView(IMarkdownService markdownService) 
            : base(markdownService)
        {
            Title = "Add Comment";
            this.WhenAnyValue(x => x.ViewModel.SaveCommand).Subscribe(x =>
                NavigationItem.RightBarButtonItem = x != null ? x.ToBarButtonItem(UIBarButtonSystemItem.Save) : null);
        }
    }
}

