using System;
using CodeHub.Core.ViewModels.Repositories;
using Foundation;
using ReactiveUI;

namespace CodeHub.iOS.Cells
{
    public class LanguageTableViewCell : ReactiveTableViewCell<LanguageItemViewModel>
    {
        public static NSString Key = new NSString("LanguageTableViewCell");

        public LanguageTableViewCell(IntPtr handle)
            : base(handle)
        {
            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel.Name).Subscribe(x=> TextLabel.Text = x));
                d(this.WhenAnyValue(x => x.ViewModel.Selected).Subscribe(x => {
                    Accessory = x ? UIKit.UITableViewCellAccessory.Checkmark : UIKit.UITableViewCellAccessory.None;
                    SetNeedsDisplay();
                }));
            });
        }
    }
}

