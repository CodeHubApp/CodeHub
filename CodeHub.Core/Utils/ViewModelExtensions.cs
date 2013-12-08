using CodeHub.Core.Services;
using CodeFramework.Core.ViewModels;

public static class ViewModelExtensions
{
	public static IApplicationService GetApplication(this BaseViewModel vm)
    {
		return vm.GetService<IApplicationService>();
    }
}

