using System.Collections.Generic;
using MonoTouch.Dialog;
using System.Linq;
using CodeHub.Core.Services;
using CodeFramework.iOS.ViewControllers;
using System;
using CodeHub.Core.ViewModels.App;
using System.Threading.Tasks;

namespace CodeHub.iOS.Views.App
{
    public class UpgradesView : ViewModelDrivenDialogViewController
    {
        private readonly List<Item> _items = new List<Item>();
        private readonly IFeaturesService _features;

        public UpgradesView()
        {
            Title = "Upgrades";
            EnableSearch = false;
            Style = UIKit.UITableViewStyle.Plain;
            _features = Cirrious.CrossCore.Mvx.Resolve<IFeaturesService>();
            NavigationItem.RightBarButtonItem = new UIKit.UIBarButtonItem("Restore", UIKit.UIBarButtonItemStyle.Plain, (s, e) => Restore());
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            var vm = (UpgradesViewModel)this.ViewModel;
            vm.Bind(x => x.Keys, x => LoadProducts(x));
        }

        private async Task LoadProducts(string[] keys)
        {
            try
            {
                MonoTouch.Utilities.PushNetworkActive();
                var data = await InAppPurchases.RequestProductData(keys);
                _items.Clear();
                _items.AddRange(data.Products.Select(x => new Item { Id = x.ProductIdentifier, Name = x.LocalizedTitle, Description = x.LocalizedDescription, Price = x.LocalizedPrice() }));
                Render();
            }
            catch (Exception e)
            {
                MonoTouch.Utilities.ShowAlert("Error", e.Message);
            }
            finally
            {
                MonoTouch.Utilities.PopNetworkActive();
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            InAppPurchases.Instance.PurchaseSuccess += HandlePurchaseSuccess;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            InAppPurchases.Instance.PurchaseSuccess -= HandlePurchaseSuccess;
        }

        void HandlePurchaseSuccess (object sender, string e)
        {
            Render();
        }

        private void Render()
        {
            var section = new Section();
            section.AddAll(_items.Select(item =>
            {
                var el = new MultilinedElement(item.Name + " (" + item.Price + ")", item.Description);
                if (_features.IsActivated(item.Id))
                {
                    el.Accessory = UIKit.UITableViewCellAccessory.Checkmark;
                }
                else
                {
                    el.Accessory = UIKit.UITableViewCellAccessory.DisclosureIndicator;
                    el.Tapped += () => Tapped(item);
                }

                return el;
            }));

            var root = new RootElement(Title) { UnevenRows = true };
            root.Add(section);
            Root = root;
        }

        private void Restore()
        {
            InAppPurchases.Instance.Restore();
        }

        private void Tapped(Item item)
        {
            _features.Activate(item.Id);
        }

        private class Item
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Price { get; set; }
        }
    }
}

