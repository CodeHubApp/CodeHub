using System;
using CodeHub.iOS.Views;
using CodeHub.Core.ViewModels.Repositories;
using MonoTouch.Foundation;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.iOS.Cells
{
    public class LanguageTableViewCell : ReactiveTableViewCell<LanguageItemViewModel>
    {
        public static NSString Key = new NSString("LanguageTableViewCell");

        public LanguageTableViewCell(IntPtr handle)
            : base(handle)
        {
            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    TextLabel.Text = x.Name;
                });

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Select(x => x.WhenAnyValue(y => y.Selected))
                .Switch()
                .Subscribe(x =>
                {
                    Accessory = x ? MonoTouch.UIKit.UITableViewCellAccessory.Checkmark : MonoTouch.UIKit.UITableViewCellAccessory.None;
                    SetNeedsDisplay();
                });
        }
    }
}

