using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using CodeHub.iOS;
using CodeHub.Core.ViewModels.Repositories;
using ReactiveUI;
using System.Reactive.Linq;
using SDWebImage;

namespace CodeHub.iOS.Cells
{
    public partial class RepositoryCellView : ReactiveTableViewCell<RepositoryItemViewModel>
    {
        public static readonly UINib Nib = UINib.FromName("RepositoryCellView", NSBundle.MainBundle);
        public static NSString Key = new NSString("RepositoryCellView");
        public static bool RoundImages = true;
        private static float DefaultConstraintSize = 0.0f;

        public RepositoryCellView(IntPtr handle)
            : base(handle)
        {
            SeparatorInset = new UIEdgeInsets(0, 56f, 0, 0);
        }

        public static RepositoryCellView Create()
        {
            return Nib.Instantiate(null, null).GetValue(0) as RepositoryCellView;
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            CaptionLabel.TextColor = Theme.MainTitleColor;
            ContentLabel.TextColor = Theme.MainTextColor;

            FollowersImageVIew.Image = Theme.CurrentTheme.RepositoryCellFollowers;
            ForksImageView.Image = Theme.CurrentTheme.RepositoryCellForks;
            UserImageView.Image = Theme.CurrentTheme.RepositoryCellUser;

            OwnerImageView.Layer.MasksToBounds = true;
            OwnerImageView.Layer.CornerRadius = OwnerImageView.Bounds.Height / 2f;
            DefaultConstraintSize = ContentConstraint.Constant;
            ContentView.Opaque = true;

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    CaptionLabel.Text = x.Name;
                    FollowersLabel.Text = x.Stars.ToString();
                    ForksLabel.Text = x.Forks.ToString();
                    OwnerImageView.SetImage(new NSUrl(x.ImageUrl), Images.LoginUserUnknown);
                    ContentLabel.Hidden = string.IsNullOrEmpty(x.Description);
                    ContentLabel.Text = x.Description ?? string.Empty;
                    UserLabel.Hidden = !x.ShowOwner || string.IsNullOrEmpty(x.Owner);
                    UserImageView.Hidden = UserLabel.Hidden;
                    UserLabel.Text = x.Owner ?? string.Empty;
                    ContentConstraint.Constant = string.IsNullOrEmpty(ContentLabel.Text) ? 0f : DefaultConstraintSize;
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

