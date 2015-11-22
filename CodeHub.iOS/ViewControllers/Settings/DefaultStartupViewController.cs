using System;
using System.Linq;
using UIKit;
using ReactiveUI;
using CodeHub.Core.ViewModels.Settings;
using CodeHub.iOS.TableViewSources;
using CodeHub.iOS.DialogElements;
using System.Reactive.Linq;

namespace CodeHub.iOS.ViewControllers.Settings
{
    public class DefaultStartupViewController : BaseTableViewController<DefaultStartupViewModel>
	{
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
            var source = new DialogTableViewSource(TableView);
            TableView.Source = source;

            OnActivation(d => {
                d(source.SelectedObservable.Cast<StringElement>()
                    .Subscribe(x => ViewModel.SelectedStartupView = x.Caption));

                d(this.WhenAnyValue(x => x.ViewModel.SelectedStartupView).Subscribe(x => {
                    var items = ViewModel.StartupViews.Select(view => {
                        var el = new StringElement(view);
                        el.Clicked.Subscribe(_ => {});
                        el.Accessory = string.Equals(view, ViewModel.SelectedStartupView, StringComparison.OrdinalIgnoreCase) 
                            ? UITableViewCellAccessory.Checkmark :  UITableViewCellAccessory.None;
                        return el;
                    });

                    source.Root.Reset(new Section { items });
                }));
            });
		}
    }
}

