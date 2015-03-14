using System;
using Foundation;
using UIKit;
using CodeHub.iOS;
using CodeHub.Core.ViewModels.Repositories;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.iOS.Cells
{
    public partial class RepositoryCellView : ReactiveTableViewCell<RepositoryItemViewModel>
    {
        public static readonly UINib Nib = UINib.FromName("RepositoryCellView", NSBundle.MainBundle);
        public static NSString Key = new NSString("RepositoryCellView");
        public static bool RoundImages = true;
        private static nfloat DefaultConstraintSize = 0.0f;

        public RepositoryCellView(IntPtr handle)
            : base(handle)
        {
            SeparatorInset = new UIEdgeInsets(0, 56f, 0, 0);
        }

        public static RepositoryCellView Create()
        {
            return Nib.Instantiate(null, null).GetValue(0) as RepositoryCellView;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            CaptionLabel.PreferredMaxLayoutWidth = CaptionLabel.Frame.Width;
            ContentLabel.PreferredMaxLayoutWidth = ContentLabel.Frame.Width;
            LayoutIfNeeded();
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            CaptionLabel.TextColor = Theme.MainTitleColor;
            ContentLabel.TextColor = Theme.MainTextColor;

            FollowersImageVIew.TintColor = FollowersLabel.TextColor;
            ForksImageView.TintColor = ForksLabel.TextColor;
            UserImageView.TintColor = UserLabel.TextColor;

            FollowersImageVIew.Image = Octicon.Star.ToImage(FollowersImageVIew.Frame.Height);
            ForksImageView.Image = Octicon.RepoForked.ToImage(ForksImageView.Frame.Height);
            UserImageView.Image = Octicon.Person.ToImage(UserImageView.Frame.Height);

            OwnerImageView.Layer.MasksToBounds = true;
            OwnerImageView.Layer.CornerRadius = OwnerImageView.Bounds.Height / 2f;
            ContentView.Opaque = true;

            DefaultConstraintSize = ContentConstraint.Constant;

            this.OneWayBind(ViewModel, x => x.Name, x => x.CaptionLabel.Text);
            this.OneWayBind(ViewModel, x => x.Stars, x => x.FollowersLabel.Text);
            this.OneWayBind(ViewModel, x => x.Forks, x => x.ForksLabel.Text);

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    OwnerImageView.SetAvatar(x.Avatar);
                    ContentLabel.Hidden = string.IsNullOrEmpty(x.Description);
                    ContentLabel.Text = x.Description ?? string.Empty;
                    UserLabel.Hidden = !x.ShowOwner || string.IsNullOrEmpty(x.Owner);
                    UserImageView.Hidden = UserLabel.Hidden;
                    UserLabel.Text = x.Owner ?? string.Empty;
                    ContentConstraint.Constant = string.IsNullOrEmpty(ContentLabel.Text) ? 0f : DefaultConstraintSize;
                });
        }
    }
}

