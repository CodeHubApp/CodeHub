using CodeHub.iOS.Views.App;
using CodeHub.Core.ViewModels.Changesets;
using Xamarin.Utilities.Core.Services;

namespace CodeHub.iOS.Views.Source
{
    public class CommitCommentView : MarkdownComposerView<CommitCommentViewModel>
    {
        public CommitCommentView(IAlertDialogService alertDialogService) 
            : base(alertDialogService)
        {
        }
    }
}

