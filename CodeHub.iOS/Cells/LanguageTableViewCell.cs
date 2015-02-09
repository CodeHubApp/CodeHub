using System;
using CodeHub.Core.ViewModels.Repositories;
using Foundation;
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
            this.OneWayBind(ViewModel, x => x.Name, x => x.TextLabel.Text);

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Select(x => x.WhenAnyValue(y => y.Selected))
                .Switch()
                .Subscribe(x =>
                {
                    Accessory = x ? UIKit.UITableViewCellAccessory.Checkmark : UIKit.UITableViewCellAccessory.None;
                    SetNeedsDisplay();
                });
        }
    }
}

