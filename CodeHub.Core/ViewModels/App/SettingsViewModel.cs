using System.Windows.Input;
using CodeHub.Core.Services;
using System;
using System.Threading.Tasks;
using CodeHub.Core.ViewModels.Repositories;
using MvvmCross.Core.ViewModels;
using System.Reactive.Subjects;
using System.Reactive;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.App
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IFeaturesService _featuresService;
        private readonly ISubject<Unit> _showUpgrades = new Subject<Unit>();

        public IObservable<Unit> ShowUpgrades => _showUpgrades.AsObservable();

        public SettingsViewModel(IFeaturesService featuresService)
        {
            _featuresService = featuresService;
        }

        public string DefaultStartupViewName
        {
            get { return this.GetApplication().Account.DefaultStartupView; }
        }

        public bool ShouldShowUpgrades
        {
            get { return _featuresService.IsProEnabled; }
        }

        public ICommand GoToSourceCodeCommand
        {
            get { return new MvxCommand(() => ShowViewModel<RepositoryViewModel>(new RepositoryViewModel.NavObject { Repository = "codehub", Username = "codehubapp" })); }
        }

        private bool _isSaving;
        public bool IsSaving
        {
            get { return _isSaving; }
            private set { this.RaiseAndSetIfChanged(ref _isSaving, value); }
        }

        public bool PushNotificationsEnabled
        {
            get 
            { 
                return this.GetApplication().Account.IsPushNotificationsEnabled.HasValue && this.GetApplication().Account.IsPushNotificationsEnabled.Value; 
            }
            set 
            { 
                if (_featuresService.IsProEnabled)
                {
                    RegisterPushNotifications(value).ToBackground();
                }
                else
                {
                    PromptNotificationActivation().ToBackground();
                    RaisePropertyChanged(() => PushNotificationsEnabled);
                }
            }
        }

        private async Task PromptNotificationActivation()
        {
            var alertDialogService = GetService<IAlertDialogService>();

            var response = await alertDialogService.PromptYesNo(
                "Requires Activation",
                "Push notifications require activation. Would you like to go there now to activate push notifications?");

            if (response)
                _showUpgrades.OnNext(Unit.Default);
        }

        private async Task RegisterPushNotifications(bool enabled)
        {
            var notificationService = GetService<IPushNotificationsService>();

            try
            {
                IsSaving = true;
                if (enabled)
                {
                    await notificationService.Register();
                }
                else
                {
                    await notificationService.Deregister();
                }

                this.GetApplication().Account.IsPushNotificationsEnabled = enabled;
                await this.GetApplication().UpdateActiveAccount();
            }
            catch (Exception e)
            {
                GetService<IAlertDialogService>()
                    .Alert("Unable to register for push notifications!", e.Message)
                    .ToBackground();
            }
            finally
            {
                RaisePropertyChanged(() => PushNotificationsEnabled);
                IsSaving = false;
            }
        }
    }
}
