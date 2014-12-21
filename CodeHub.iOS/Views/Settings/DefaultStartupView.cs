using System;
using System.Linq;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;
using Xamarin.Utilities.Delegates;
using CodeHub.Core.ViewModels.Settings;

namespace CodeHub.iOS.Views.Settings
{
    public class DefaultStartupView : ReactiveTableViewController<DefaultStartupViewModel>
	{
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
            var source = new DialogTableViewSource(TableView);
            TableView.Source = source;

            ViewModel.WhenAnyValue(x => x.SelectedStartupView).Subscribe(x =>
            {
                var items = ViewModel.StartupViews.Select(view => 
                {
                    var el = new StyledStringElement(view, () => ViewModel.SelectedStartupView = view);
                    el.Accessory = string.Equals(view, ViewModel.SelectedStartupView, StringComparison.OrdinalIgnoreCase) 
                        ? UITableViewCellAccessory.Checkmark :  UITableViewCellAccessory.None;
                    return el;
                });

                source.Root.Reset(new Section { items });
            });
		}
    }
}

