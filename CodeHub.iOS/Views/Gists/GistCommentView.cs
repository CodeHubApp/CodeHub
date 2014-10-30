using CodeHub.Core.ViewModels.Gists;
using Xamarin.Utilities.Core.Services;
using CodeHub.iOS.Views.App;

namespace CodeHub.iOS.Views.Gists
{
    public class GistCommentView : MarkdownComposerView<GistCommentViewModel>
    {
        public GistCommentView(IAlertDialogService alertDialogService) 
            : base(alertDialogService)
        {
        }
    }
}

