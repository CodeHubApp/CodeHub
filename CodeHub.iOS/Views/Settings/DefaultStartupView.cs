using System;
using System.Linq;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.Core.ViewModels.Settings;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.Views.Settings
{
    public class DefaultStartupView : BaseTableViewController<DefaultStartupViewModel>
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

