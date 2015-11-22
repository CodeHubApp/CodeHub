using CodeHub.iOS.ViewControllers.App;
using ReactiveUI;
using UIKit;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels;

namespace CodeHub.iOS.ViewControllers
{
    public class ComposerViewController : MarkdownComposerViewController<ComposerViewModel>, IModalViewController
    {
        public ComposerViewController(IMarkdownService markdownService) 
            : base(markdownService)
        {
            OnActivation(d => {
                d(this.WhenAnyValue(x => x.ViewModel.SaveCommand)
                    .ToBarButtonItem(UIBarButtonSystemItem.Save, x => NavigationItem.RightBarButtonItem = x));
                d(this.WhenAnyValue(x => x.ViewModel.DismissCommand)
                    .ToBarButtonItem(Images.Cancel, x => NavigationItem.LeftBarButtonItem = x));
            });
        }
    }
}

