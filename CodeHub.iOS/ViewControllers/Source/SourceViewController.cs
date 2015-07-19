using CodeHub.Core.ViewModels.Source;
using CodeHub.Core.Factories;

namespace CodeHub.iOS.ViewControllers.Source
{
	public class SourceViewController : FileSourceViewController<SourceViewModel>
    {
        public SourceViewController(IAlertDialogFactory alertDialogFactory)
            : base(alertDialogFactory)
        {
        }
    }
}

