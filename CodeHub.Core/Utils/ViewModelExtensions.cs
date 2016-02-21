using CodeHub.Core.Services;
using CodeHub.Core.ViewModels;
using MvvmCross.Platform;

public static class ViewModelExtensions
{
    public static IApplicationService GetApplication(this BaseViewModel vm)
    {
        return Mvx.Resolve<IApplicationService>();
    }
}

