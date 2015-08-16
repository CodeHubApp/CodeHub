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
        private bool _fakeCell;

        public RepositoryCellView(IntPtr handle)
            : base(handle)
        {
            SeparatorInset = new UIEdgeInsets(0, 56f, 0, 0);
        }

        public static RepositoryCellView Create(bool fakeCell = false)
        {
            var cell = Nib.Instantiate(null, null).GetValue(0) as RepositoryCellView;
            cell._fakeCell = fakeCell;
            return cell;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            ContentView.SetNeedsLayout();
            ContentView.LayoutIfNeeded();

            CaptionLabel.PreferredMaxLayoutWidth = CaptionLabel.Frame.Width;
            ContentLabel.PreferredMaxLayoutWidth = ContentLabel.Frame.Width;
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

            OwnerImageView.Layer.CornerRadius = OwnerImageView.Bounds.Height / 2f;
            OwnerImageView.Layer.MasksToBounds = true;
            OwnerImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            ContentView.Opaque = true;

            DefaultConstraintSize = ContentConstraint.Constant;

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x => {
                    CaptionLabel.Text = x.Name;
                    FollowersLabel.Text = x.Stars.ToString();
                    ForksLabel.Text = x.Forks.ToString();
                    ContentLabel.Hidden = string.IsNullOrEmpty(x.Description);
                    ContentLabel.Text = x.Description ?? string.Empty;
                    UserLabel.Hidden = !x.ShowOwner || string.IsNullOrEmpty(x.Owner);
                    UserImageView.Hidden = UserLabel.Hidden;
                    UserLabel.Text = x.Owner ?? string.Empty;
                    ContentConstraint.Constant = string.IsNullOrEmpty(ContentLabel.Text) ? 0f : DefaultConstraintSize;
                });

            this.WhenAnyValue(x => x.ViewModel.Avatar)
                .Where(_ => !_fakeCell)
                .Subscribe(x => OwnerImageView.SetAvatar(x));
        }
    }
}

