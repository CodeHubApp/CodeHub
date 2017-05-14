using System;
using Foundation;
using UIKit;
using CodeHub.Core.ViewModels.Repositories;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.iOS.TableViewCells
{
    public partial class RepositoryCellView : ReactiveTableViewCell<RepositoryItemViewModel>
    {
        public static readonly UINib Nib = UINib.FromName("RepositoryCellView", NSBundle.MainBundle);
        public static NSString Key = new NSString("RepositoryCellView");
        private static nfloat DefaultConstraintSize = 0.0f;

        public RepositoryCellView(IntPtr handle)
            : base(handle)
        {
            SeparatorInset = new UIEdgeInsets(0, 56f, 0, 0);
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            CaptionLabel.TextColor = Theme.CurrentTheme.MainTitleColor;
            ContentLabel.TextColor = Theme.CurrentTheme.MainTextColor;

            FollowersImageVIew.TintColor = FollowersLabel.TextColor;
            ForksImageView.TintColor = ForksLabel.TextColor;
            UserImageView.TintColor = UserLabel.TextColor;

            FollowersImageVIew.Image = Octicon.Star.ToImage(FollowersImageVIew.Frame.Height);
            ForksImageView.Image = Octicon.RepoForked.ToImage(ForksImageView.Frame.Height);
            UserImageView.Image = Octicon.Person.ToImage(UserImageView.Frame.Height);

            OwnerImageView.Layer.CornerRadius = OwnerImageView.Bounds.Height / 2f;
            OwnerImageView.Layer.MasksToBounds = true;
            OwnerImageView.ContentMode = UIViewContentMode.ScaleAspectFill;

            DefaultConstraintSize = ContentConstraint.Constant;

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    CaptionLabel.Text = x.Name;
                    FollowersLabel.Text = x.Stars;
                    ForksLabel.Text = x.Forks;
                    ContentLabel.Hidden = string.IsNullOrEmpty(x.Description);
                    ContentLabel.Text = x.Description ?? string.Empty;
                    UserLabel.Hidden = !x.ShowOwner || string.IsNullOrEmpty(x.Owner);
                    UserImageView.Hidden = UserLabel.Hidden;
                    UserLabel.Text = x.Owner ?? string.Empty;
                    ContentConstraint.Constant = string.IsNullOrEmpty(ContentLabel.Text) ? 0f : DefaultConstraintSize;
                    OwnerImageView.SetAvatar(x.Avatar);
                });
        }

        protected override void Dispose(bool disposing)
        {
            ReleaseDesignerOutlets();
            base.Dispose(disposing);
        }
    }
}

