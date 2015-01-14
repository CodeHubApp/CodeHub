using CodeHub.Core.ViewModels.Source;
using CodeHub.Core.Services;
using CodeHub.Core.Factories;

namespace CodeHub.iOS.Views.Source
{
	public class SourceView : FileSourceView<SourceViewModel>
    {
        public SourceView(INetworkActivityService networkActivityService, IAlertDialogFactory alertDialogFactory)
            : base(networkActivityService, alertDialogFactory)
        {
        }
    }
}

