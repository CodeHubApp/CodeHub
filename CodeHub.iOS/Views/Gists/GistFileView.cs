using System;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.iOS.Views.Source;
using CodeHub.Core.Services;
using CodeHub.Core.Factories;

namespace CodeHub.iOS.Views.Gists
{
	public class GistFileView : FileSourceView<GistFileViewModel>
    {
        public GistFileView(INetworkActivityService networkActivityService, IAlertDialogFactory alertDialogFactory)
            : base(networkActivityService, alertDialogFactory)
        {
        }

        protected override void LoadSource(Uri fileUri)
        {
            if (ViewModel.GistFile != null)
                base.LoadSource(fileUri);
        }
    }
}

