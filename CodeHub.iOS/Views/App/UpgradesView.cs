using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Factories;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.App;
using ReactiveUI;
using Splat;
using Xamarin.Utilities.Services;

namespace CodeHub.iOS.Views.App
{
    public class UpgradesView : BaseTableViewController<UpgradesViewModel>
    {
        private readonly IFeaturesService _featuresService;
        private readonly INetworkActivityService _networkActivityService;
        private readonly IAlertDialogFactory _alertDialogService;
        private readonly List<Item> _items = new List<Item>();

        public UpgradesView(IFeaturesService featuresService, INetworkActivityService networkActivityService, IAlertDialogFactory alertDialogService)
        {
            _featuresService = featuresService;
            _networkActivityService = networkActivityService;
            _alertDialogService = alertDialogService;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.RightBarButtonItem = new MonoTouch.UIKit.UIBarButtonItem("Restore", MonoTouch.UIKit.UIBarButtonItemStyle.Plain, (s, e) => Restore());
            ViewModel.WhenAnyValue(x => x.Keys).Where(x => x != null && x.Length > 0).Subscribe(x => LoadProducts(x));
        }

        private async Task LoadProducts(string[] keys)
        {
            try
            {
                _networkActivityService.PushNetworkActive();
                var data = await Locator.Current.GetService<InAppPurchaseService>().RequestProductData(keys);
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

        private void Render()
        {
//            var section = new Section();
//            section.AddAll(_items.Select(item =>
//            {
//                var el = new MultilinedElement(item.Name + " (" + item.Price + ")", item.Description);
//                if (_featuresService.IsActivated(item.Id))
//                {
//                    el.Accessory = MonoTouch.UIKit.UITableViewCellAccessory.Checkmark;
//                }
//                else
//                {
//                    el.Accessory = MonoTouch.UIKit.UITableViewCellAccessory.DisclosureIndicator;
//                    el.Tapped += () => Tapped(item);
//                }
//
//                return el;
//            }));
//
//            Root.Reset(section);
        }

        private void Restore()
        {
            Locator.Current.GetService<InAppPurchaseService>().Restore();
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

