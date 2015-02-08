using System;
using CodeHub.Core.ViewModels.Issues;
using Foundation;
using ReactiveUI;
using System.Reactive.Linq;
using UIKit;
using CodeHub.iOS.Utilities;

namespace CodeHub.iOS.Cells
{
    public class IssueLabelCellView : ReactiveTableViewCell<IssueLabelItemViewModel>
    {
        public static NSString Key = new NSString("IssueLabelCellView");

        public IssueLabelCellView(IntPtr handle)
            : base(handle)
        {
            this.WhenAnyValue(x => x.ViewModel)
                .IsNotNull()
                .Subscribe(x =>
                {
                    TextLabel.Text = x.Name;
                    ImageView.Image = Graphics.CreateLabelImage(x.Color);
                });

            this.WhenAnyValue(x => x.ViewModel)
                .IsNotNull()
                .Select(x => x.WhenAnyValue(y => y.IsSelected))
                .Switch()
                .Subscribe(x =>
                {
                    Accessory = x ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
                });
        }
    }
}

