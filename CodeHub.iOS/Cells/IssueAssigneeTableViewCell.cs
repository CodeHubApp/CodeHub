using System;
using Foundation;
using CoreGraphics;
using UIKit;
using System.Reactive.Linq;
using ReactiveUI;
using CodeHub.Core.ViewModels.Issues;

namespace CodeHub.iOS.Cells
{
    public class IssueAssigneeTableViewCell : ReactiveTableViewCell<IssueAssigneeItemViewModel>
    {
        public static NSString Key = new NSString("assigneecell");
        private const float ImageSpacing = 10f;
        private static CGRect ImageFrame = new CGRect(ImageSpacing, 6f, 32, 32);

        public IssueAssigneeTableViewCell(IntPtr handle)
            : base(handle)
        { 
            SeparatorInset = new UIEdgeInsets(0, ImageFrame.Right + ImageSpacing, 0, 0);
            ImageView.Layer.CornerRadius = ImageFrame.Height / 2f;
            ImageView.Layer.MasksToBounds = true;
            ImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            ContentView.Opaque = true;

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel).IsNotNull().Subscribe(x => {
                    TextLabel.Text = x.Name;
                    ImageView.SetAvatar(x.Avatar);
                }));

                d(this.WhenAnyValue(x => x.ViewModel.IsSelected)
                    .Subscribe(x => Accessory = x ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None));
            });
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            ImageView.Frame = ImageFrame;
            TextLabel.Frame = new CGRect(ImageFrame.Right + ImageSpacing, TextLabel.Frame.Y, TextLabel.Frame.Width, TextLabel.Frame.Height);
        }
    }
}

