using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels.App;
using System;
using UIKit;
using System.Linq;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.ViewControllers.Application
{
    public class DefaultStartupViewController : ViewModelCollectionDrivenDialogViewController
    {
        public DefaultStartupViewController()
        {
            Title = "Default Startup View";
            EnableSearch = false;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var vm = (BaseDefaultStartupViewModel)ViewModel;
            BindCollection(vm.StartupViews, x => {
                var e = new StringElement(x);
                e.Clicked.Subscribe(_ => vm.SelectedStartupView = x);
                if (string.Equals(vm.SelectedStartupView, x))
                    e.Accessory = UITableViewCellAccessory.Checkmark;
                return e;
            }, true);

            vm.Bind(x => x.SelectedStartupView, true).Subscribe(x =>
            {
                if (Root.Count == 0)
                    return;
                foreach (var m in Root[0].Elements.Cast<StringElement>())
                    m.Accessory = (string.Equals(m.Caption, x)) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
            });
        }
    }
}

