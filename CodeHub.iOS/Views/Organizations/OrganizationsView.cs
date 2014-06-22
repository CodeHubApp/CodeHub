using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Organizations;
using CodeFramework.iOS.Elements;
using ReactiveUI;

namespace CodeHub.iOS.Views.Organizations
{
    public class OrganizationsView : ViewModelCollectionView<OrganizationsViewModel>
    {
        public OrganizationsView()
            : base("Organizations")
        {
            NoItemsText = "No Organizations";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			Bind(ViewModel.WhenAnyValue(x => x.Organizations), x =>
			{
				var e = new UserElement(x.Login, string.Empty, string.Empty, x.AvatarUrl);
				e.Tapped += () => ViewModel.GoToOrganizationCommand.Execute(x);
				return e;
			});
        }
	}
}

