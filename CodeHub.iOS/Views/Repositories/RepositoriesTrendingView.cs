using System;
using System.Linq;
using System.Reactive.Linq;
using CodeFramework.iOS.Elements;
using CodeHub.Core.ViewModels.Repositories;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.Views;
using Xamarin.Utilities.ViewControllers;

namespace CodeHub.iOS.Views.Repositories
{
    public class RepositoriesTrendingView : ViewModelCollectionViewController<RepositoriesTrendingViewModel>
    {
        public RepositoriesTrendingView()
            : base(unevenRows: true, searchbarEnabled: false)
        {
            Title = "Trending";
        }

        public override void ViewDidLoad()
        {
            //NoItemsText = "No Repositories";

            base.ViewDidLoad();

            this.BindList(ViewModel.Repositories, repo =>
            {
				var description = ViewModel.ShowRepositoryDescription ? repo.Description : string.Empty;
                var imageUrl = new Uri(repo.AvatarUrl);
                var sse = new RepositoryElement(repo.Name, repo.Stars, repo.Forks, description, repo.Owner, imageUrl) { ShowOwner = true };
                sse.Tapped += () => ViewModel.GoToRepositoryCommand.Execute(repo);
                return sse;
            });

            var button = new UIBarButtonItem("Time", UIBarButtonItemStyle.Plain, (s, e) =>
            {
                var index = ViewModel.SelectedTime == null ? 0 : ViewModel.Times.ToList().IndexOf(ViewModel.SelectedTime);
                if (index < 0) index = 0;
                new PickerAlertView(ViewModel.Times.Select(x => x.Name).ToArray(), index, x => 
                {
                    var selectedTime = ViewModel.Times.ElementAtOrDefault(x);
                    if (selectedTime != null)
                        ViewModel.SelectedTime = selectedTime;
                }).Show();
            });

            var button2 = new UIBarButtonItem("Language", UIBarButtonItemStyle.Plain, (s, e) =>
            {
                var index = ViewModel.SelectedLanguage == null ? 0 : ViewModel.Languages.ToList().IndexOf(ViewModel.SelectedLanguage);
                if (index < 0) index = 0;
                new PickerAlertView(ViewModel.Languages.Select(x => x.Name).ToArray(), index, x =>
                {
                    var selectedlanguage = ViewModel.Languages.ElementAtOrDefault(x);
                    if (selectedlanguage != null)
                        ViewModel.SelectedLanguage = selectedlanguage;
                }).Show();
            });

            ViewModel.WhenAnyValue(x => x.SelectedTime)
                .Where(x => x != null)
                .Subscribe(x => button.Title = x.Name);

            ViewModel.WhenAnyValue(x => x.SelectedLanguage)
                .Where(x => x != null)
                .Subscribe(x => button2.Title = x.Name);

            ToolbarItems = new[]
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

