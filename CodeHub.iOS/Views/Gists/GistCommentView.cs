using CodeHub.Core.ViewModels.Gists;
using CodeHub.iOS.Views.App;
using Xamarin.Utilities.Factories;

namespace CodeHub.iOS.Views.Gists
{
    public class GistCommentView : MarkdownComposerView<GistCommentViewModel>
    {
        public GistCommentView(IAlertDialogFactory alertDialogService) 
            : base(alertDialogService)
        {
        }
    }
}

