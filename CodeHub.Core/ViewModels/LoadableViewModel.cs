using GitHubSharp;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;

namespace CodeHub.Core.ViewModels
{
    public abstract class LoadableViewModel : CodeFramework.Core.ViewModels.LoadableViewModel
    {
        protected override async Task ExecuteLoadResource(bool forceCacheInvalidation)
        {
            try
            {
                await base.ExecuteLoadResource(forceCacheInvalidation);
            }
            catch (System.IO.IOException)
            {
                DisplayAlert("Unable to communicate with GitHub as the transmission was interrupted! Please try again.");
            }
            catch (StatusCodeException e)
            {
                DisplayAlert(e.Message);

                if (e is NotFoundException)
                    ChangePresentation(new MvxClosePresentationHint(this));
            }
        }
    }
}

