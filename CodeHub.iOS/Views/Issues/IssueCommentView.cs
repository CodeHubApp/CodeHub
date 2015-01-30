using System;
using CodeHub.iOS.Views.App;
using ReactiveUI;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using CodeHub.Core.Services;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Issues
{
    public class IssueCommentView : MarkdownComposerView<IssueCommentViewModel>
    {
        public IssueCommentView(IMarkdownService markdownService) 
            : base(markdownService)
        {
            this.WhenAnyValue(x => x.ViewModel.SaveCommand)
                .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Save))
                .Subscribe(x => NavigationItem.RightBarButtonItem = x);
        }
    }
}

