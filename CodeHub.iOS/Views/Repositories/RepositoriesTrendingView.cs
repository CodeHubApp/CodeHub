using System;
using CodeFramework.iOS.Elements;
using CodeFramework.ViewControllers;
using CodeFramework.iOS.Utils;
using CodeHub.Core.ViewModels.Repositories;
using UIKit;
using System.Linq;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoriesTrendingView : ViewModelCollectionDrivenDialogViewController
    {
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
                var imageUrl = Images.GitHubRepoUrl;
                var sse = new RepositoryElement(repo.Name, repo.Stars, repo.Forks, description, repo.Owner, imageUrl) { ShowOwner = true };
				sse.Tapped += () => vm.GoToRepositoryCommand.Execute(repo);
                return sse;
            });

            var button = new UIBarButtonItem("Time", UIBarButtonItemStyle.Plain, (s, e) =>
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

            var button2 = new UIBarButtonItem("Language", UIBarButtonItemStyle.Plain, (s, e) =>
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

            vm.Bind(x => x.SelectedTime, x => button.Title = x.Name, true);
            vm.Bind(x => x.SelectedLanguage, x => button2.Title = x.Name, true);

            ToolbarItems = new UIBarButtonItem[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                button2,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                button,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
        }
    }
}

