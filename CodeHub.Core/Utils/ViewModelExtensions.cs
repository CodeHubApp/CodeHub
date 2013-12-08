using CodeHub.Core.Services;
using CodeFramework.Core.ViewModels;
using Cirrious.CrossCore;

public static class ViewModelExtensions
{
	public static IApplicationService GetApplication(this BaseViewModel vm)
    {
		return Mvx.Resolve<IApplicationService>();
    }
}

