using System;
using CodeHub.Core.ViewModels.Repositories;
using ReactiveUI;
using Xamarin.Utilities.Core.Services;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.App
{
	public class AboutViewModel : BaseViewModel
    {
        private readonly IEnvironmentalService _environmentService;

        public string Version
        {
            get { return _environmentService.ApplicationVersion; }
        }

        public IReactiveCommand GoToSourceCodeCommand { get; private set; }

        public AboutViewModel(IEnvironmentalService environmentService)
        {
            _environmentService = environmentService;

            GoToSourceCodeCommand = new ReactiveCommand();
            GoToSourceCodeCommand.Subscribe(x =>
            {
                var vm = CreateViewModel<RepositoryViewModel>();
                vm.RepositoryOwner = "thedillonb";
                vm.RepositoryName = "codehub";
                ShowViewModel(vm);
            });
        }
    }
}

