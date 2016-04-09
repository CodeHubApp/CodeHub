using CodeHub.Core.ViewModels;
using System.Windows.Input;
using CodeHub.Core.Services;
using System;
using System.Threading.Tasks;
using CodeHub.Core.ViewModels.Repositories;
using MvvmCross.Core.ViewModels;

namespace CodeHub.Core.ViewModels.App
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IFeaturesService _featuresService;

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

        public ICommand GoToDefaultStartupViewCommand
        {
            get { return new MvxCommand(() => ShowViewModel<DefaultStartupViewModel>()); }
        }

        public ICommand GoToSourceCodeCommand
        {
            get { return new MvxCommand(() => ShowViewModel<RepositoryViewModel>(new RepositoryViewModel.NavObject { Repository = "codehub", Username = "thedillonb" })); }
        }

        public ICommand GoToUpgradesCommand
        {
            get { return new MvxCommand(() => ShowViewModel<UpgradeViewModel>()); }
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
                    GetService<IAlertDialogService>()
                        .PromptYesNo("Requires Activation", "Push notifications require activation. Would you like to go there now to activate push notifications?")
                        .ContinueWith(t =>
                        {
                            if (t.Status == TaskStatus.RanToCompletion && t.Result)
                                ShowViewModel<UpgradeViewModel>();
                        });
                    RaisePropertyChanged(() => PushNotificationsEnabled);
                }
            }
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
                this.GetApplication().Accounts.Update(this.GetApplication().Account);
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
