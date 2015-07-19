using System;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.iOS.ViewControllers.Source;
using CodeHub.Core.Factories;

namespace CodeHub.iOS.ViewControllers.Gists
{
	public class GistFileViewController : FileSourceViewController<GistFileViewModel>
    {
        public GistFileViewController(IAlertDialogFactory alertDialogFactory)
            : base(alertDialogFactory)
        {
        }

        protected override void LoadSource(Uri fileUri)
        {
            if (ViewModel.GistFile != null)
                base.LoadSource(fileUri);
        }
    }
}

