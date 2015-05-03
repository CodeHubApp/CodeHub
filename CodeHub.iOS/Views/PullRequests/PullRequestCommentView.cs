using System;
using CodeHub.iOS.Views.App;
using CodeHub.Core.ViewModels.PullRequests;
using CodeHub.Core.Services;
using ReactiveUI;
using UIKit;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestCommentView : MarkdownComposerView<PullRequestCommentViewModel>, IModalView
    {
        public PullRequestCommentView(IMarkdownService markdownService) 
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

