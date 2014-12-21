using CodeHub.Core.ViewModels.Source;
using Xamarin.Utilities.Services;
using Xamarin.Utilities.Factories;

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

