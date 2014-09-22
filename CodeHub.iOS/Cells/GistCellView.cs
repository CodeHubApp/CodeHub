using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using CodeHub.Core.ViewModels.Gists;
using ReactiveUI;
using System.Reactive.Linq;
using SDWebImage;

namespace CodeHub.iOS.Cells
{
    public partial class GistCellView : ReactiveTableViewCell<GistItemViewModel>
    {
        public static readonly UINib Nib = UINib.FromName("GistCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("GistCellView");
        private static float DefaultContentConstraintSize = 0.0f;

        public GistCellView(IntPtr handle) 
            : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            MainImageView.Layer.MasksToBounds = true;
            MainImageView.Layer.CornerRadius = MainImageView.Frame.Height / 2f;
            SeparatorInset = new UIEdgeInsets(0, TitleLabel.Frame.Left, 0, 0);
            TitleLabel.TextColor = UIColor.FromRGB(0, 64, 128);
            TimeLabel.TextColor = UIColor.Gray;
            ContentLabel.TextColor = UIColor.FromRGB(41, 41, 41);
            DefaultContentConstraintSize = ContentConstraint.Constant;

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    MainImageView.SetImage(new NSUrl(x.ImageUrl), Images.LoginUserUnknown);
                    TitleLabel.Text = x.Owner;
                    ContentLabel.Text = x.Description;
                    TimeLabel.Text = x.UpdatedAt.ToDaysAgo();
                    ContentConstraint.Constant = string.IsNullOrEmpty(x.Description) ? 0f : DefaultContentConstraintSize;
                });
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            ContentView.SetNeedsLayout();
            ContentView.LayoutIfNeeded();
            ContentLabel.PreferredMaxLayoutWidth = ContentLabel.Frame.Width;
        }
    }
}

