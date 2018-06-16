using CodeHub.Core.Services;
using CodeHub.Core.ViewModels;
using Splat;

public static class ViewModelExtensions
{
    public static IApplicationService GetApplication(this BaseViewModel vm)
    {
        return Locator.Current.GetService<IApplicationService>();
    }
}

