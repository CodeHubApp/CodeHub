using System;
using CodeHub.Core.ViewModels.Organizations;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using CodeHub.iOS.Elements;

namespace CodeHub.iOS.Views.Organizations
{
    public class OrganizationsView : ViewModelCollectionViewController<OrganizationsViewModel>
    {
        public OrganizationsView()
        {
            Title = "Organizations";
            //NoItemsText = "No Organizations";

            this.WhenActivated(d =>
            {
                d(SearchTextChanging.Subscribe(x => ViewModel.SearchKeyword = x));
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.BindList(ViewModel.Organizations, x =>
			{
				var e = new UserElement(x.Login, string.Empty, string.Empty, x.AvatarUrl);
				e.Tapped += () => ViewModel.GoToOrganizationCommand.Execute(x);
				return e;
			});
        }
	}
}

