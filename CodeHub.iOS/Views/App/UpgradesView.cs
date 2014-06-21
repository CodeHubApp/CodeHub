using System.Collections.Generic;
using System.Reactive.Linq;
using CodeFramework.iOS.Views;
using MonoTouch.Dialog;
using System.Linq;
using CodeHub.Core.Services;
using System;
using CodeHub.Core.ViewModels.App;
using System.Threading.Tasks;
using ReactiveUI;
using Xamarin.Utilities.Core.Services;

namespace CodeHub.iOS.Views.App
{
    public class UpgradesView : ViewModelDialogView<UpgradesViewModel>
    {
        private readonly IFeaturesService _featuresService;
        private readonly INetworkActivityService _networkActivityService;
        private readonly IAlertDialogService _alertDialogService;
        private readonly List<Item> _items = new List<Item>();

        public UpgradesView(IFeaturesService featuresService, INetworkActivityService networkActivityService, IAlertDialogService alertDialogService)
        {
            _featuresService = featuresService;
            _networkActivityService = networkActivityService;
            _alertDialogService = alertDialogService;
            EnableSearch = false;
            Style = MonoTouch.UIKit.UITableViewStyle.Plain;
        }

        public override void ViewDidLoad()
        {
            Title = "Upgrades";
            NavigationItem.RightBarButtonItem = new MonoTouch.UIKit.UIBarButtonItem("Restore", MonoTouch.UIKit.UIBarButtonItemStyle.Plain, (s, e) => Restore());

            base.ViewDidLoad();

            ViewModel.WhenAnyValue(x => x.Keys).Where(x => x != null && x.Length > 0).Subscribe(x => LoadProducts(x));
        }

        private async Task LoadProducts(string[] keys)
        {
            try
            {
                _networkActivityService.PushNetworkActive();
                var data = await InAppPurchases.RequestProductData(keys);
                _items.Clear();
                _items.AddRange(data.Products.Select(x => new Item { Id = x.ProductIdentifier, Name = x.LocalizedTitle, Description = x.LocalizedDescription, Price = x.LocalizedPrice() }));
                Render();
            }
            catch (Exception e)
            {
                _alertDialogService.Alert("Error", e.Message);
            }
            finally
            {
                _networkActivityService.PopNetworkActive();
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
                if (_featuresService.IsActivated(item.Id))
                {
                    el.Accessory = MonoTouch.UIKit.UITableViewCellAccessory.Checkmark;
                }
                else
                {
                    el.Accessory = MonoTouch.UIKit.UITableViewCellAccessory.DisclosureIndicator;
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
            _featuresService.Activate(item.Id);
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

