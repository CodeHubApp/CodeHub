using CodeHub.iOS.Views.App;
using CodeHub.Core.ViewModels.Changesets;
using Xamarin.Utilities.Factories;

namespace CodeHub.iOS.Views.Source
{
    public class CommitCommentView : MarkdownComposerView<CommitCommentViewModel>
    {
        public CommitCommentView(IAlertDialogFactory alertDialogService) 
            : base(alertDialogService)
        {
        }
    }
}

