using System;
using CodeHub.Core.ViewModels.Accounts;
using Foundation;
using UIKit;
using CoreGraphics;
using System.Reactive.Linq;
using ReactiveUI;
using SDWebImage;

namespace CodeHub.iOS.Cells
{
    public class AccountCellView : ReactiveTableViewCell<AccountItemViewModel>
    {
        public static NSString Key = new NSString("ProfileCell");
        public new readonly UIImageView ImageView;
        public readonly UILabel TitleLabel;
        public readonly UILabel SubtitleLabel;

        public AccountCellView(IntPtr handle)
            : base(handle)
        {
            ImageView = new UIImageView();
            ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            ImageView.Layer.MinificationFilter = CoreAnimation.CALayer.FilterTrilinear;
            ImageView.Layer.MasksToBounds = true;

            TitleLabel = new UILabel();
            TitleLabel.TextColor = UIColor.FromWhiteAlpha(0.0f, 1f);
            TitleLabel.Font = UIFont.FromName("HelveticaNeue", 17f);

            SubtitleLabel = new UILabel();
            SubtitleLabel.TextColor = UIColor.FromWhiteAlpha(0.1f, 1f);
            SubtitleLabel.Font = UIFont.FromName("HelveticaNeue-Thin", 14f);

            ContentView.Add(ImageView);
            ContentView.Add(TitleLabel);
            ContentView.Add(SubtitleLabel);

            this.OneWayBind(ViewModel, x => x.Username, x => x.TitleLabel.Text);
            this.OneWayBind(ViewModel, x => x.Domain, x => x.SubtitleLabel.Text);

            this.WhenAnyValue(x => x.ViewModel.AvatarUrl).Where(x => !string.IsNullOrEmpty(x)).Subscribe(x => ImageView.SetImage(new NSUrl(x), Images.UnknownUser));
            this.WhenAnyValue(x => x.ViewModel.AvatarUrl).Where(x => string.IsNullOrEmpty(x)).Subscribe(_ => ImageView.Image = Images.UnknownUser);
            this.WhenAnyValue(x => x.ViewModel.Selected).Subscribe(x => Accessory = x ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var imageSize = this.Bounds.Height - 30f;
            ImageView.Layer.CornerRadius = imageSize / 2;
            ImageView.Frame = new CGRect(15, 15, imageSize, imageSize);

            var titlePoint = new CGPoint(ImageView.Frame.Right + 15f, 19f);
            TitleLabel.Frame = new CGRect(titlePoint.X, titlePoint.Y, this.ContentView.Bounds.Width - titlePoint.X - 10f, TitleLabel.Font.LineHeight);
            SubtitleLabel.Frame = new CGRect(titlePoint.X, TitleLabel.Frame.Bottom, this.ContentView.Bounds.Width - titlePoint.X - 10f, SubtitleLabel.Font.LineHeight + 1);
        }
    }
}

