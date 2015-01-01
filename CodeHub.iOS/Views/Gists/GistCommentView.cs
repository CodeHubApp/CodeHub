using CodeHub.Core.ViewModels.Gists;
using CodeHub.iOS.Views.App;
using ReactiveUI;
using MonoTouch.UIKit;
using System;
using CodeHub.Core.Services;

namespace CodeHub.iOS.Views.Gists
{
    public class GistCommentView : MarkdownComposerView<GistCommentViewModel>
    {
        public GistCommentView(IMarkdownService markdownService) 
            : base(markdownService)
        {
            this.WhenAnyValue(x => x.ViewModel.SaveCommand).Subscribe(x =>
                NavigationItem.RightBarButtonItem = x != null ? x.ToBarButtonItem(UIBarButtonSystemItem.Save) : null);
        }
    }
}

