using System;
using CodeHub.iOS.Elements;
using CodeHub.iOS.ViewControllers;
using CodeHub.iOS.Utilities;
using CodeHub.Core.ViewModels.Repositories;
using UIKit;
using System.Linq;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoriesTrendingView : ViewModelCollectionDrivenDialogViewController
    {
        private UIBarButtonItem _timeButton;
        private UIBarButtonItem _languageButton;


        public RepositoriesTrendingView()
        {
            EnableSearch = false;
            NoItemsText = "No Repositories".t();
            Title = "Trending".t();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            var vm = (RepositoriesTrendingViewModel)ViewModel;

			BindCollection(vm.Repositories, repo =>
            {
				var description = vm.ShowRepositoryDescription ? repo.Description : string.Empty;
                string imageUrl = repo.AvatarUrl;
                var sse = new RepositoryElement(repo.Name, repo.Stars, repo.Forks, description, repo.Owner, imageUrl) { ShowOwner = true };
				sse.Tapped += () => vm.GoToRepositoryCommand.Execute(repo);
                return sse;
            });

            _timeButton = new UIBarButtonItem("Time", UIBarButtonItemStyle.Plain, (s, e) =>
            {
                var index = vm.SelectedTime == null ? 0 : vm.Times.ToList().IndexOf(vm.SelectedTime);
                if (index < 0) index = 0;
                new PickerAlert(vm.Times.Select(x => x.Name).ToArray(), index, x => 
                {
                    var selectedTime = vm.Times.ElementAtOrDefault(x);
                    if (selectedTime != null)
                        vm.SelectedTime = selectedTime;
                }).Show();
            });

            _languageButton = new UIBarButtonItem("Language", UIBarButtonItemStyle.Plain, (s, e) =>
            {
                var index = vm.SelectedLanguage == null ? 0 : vm.Languages.ToList().IndexOf(vm.SelectedLanguage);
                if (index < 0) index = 0;
                new PickerAlert(vm.Languages.Select(x => x.Name).ToArray(), index, x =>
                {
                    var selectedlanguage = vm.Languages.ElementAtOrDefault(x);
                    if (selectedlanguage != null)
                        vm.SelectedLanguage = selectedlanguage;
                }).Show();
            });

            vm.Bind(x => x.SelectedTime, x => _timeButton.Title = x.Name, true);
            vm.Bind(x => x.SelectedLanguage, x => _languageButton.Title = x.Name, true);


        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            ToolbarItems = new []
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _languageButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _timeButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

            NavigationController.SetToolbarHidden(false, animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            ToolbarItems = new UIBarButtonItem[0];
            NavigationController.SetToolbarHidden(true, animated);
        }
    }
}

