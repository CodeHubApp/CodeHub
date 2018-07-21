using System;
using Foundation;
using UIKit;
using CodeHub.Core.ViewModels.Repositories;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Core.Utilities;
using ObjCRuntime;

namespace CodeHub.iOS.TableViewCells
{
    public partial class RepositoryCellView : UITableViewCell
    {
        public static readonly UINib Nib = UINib.FromName("RepositoryCellView", NSBundle.MainBundle);
        public static NSString Key = new NSString("RepositoryCellView");
        private static nfloat DefaultConstraintSize = 0.0f;

        public RepositoryCellView()
        {
            SeparatorInset = new UIEdgeInsets(0, 56f, 0, 0);
        }

        public RepositoryCellView(IntPtr handle)
            : base(handle)
        {
            SeparatorInset = new UIEdgeInsets(0, 56f, 0, 0);
        }

        public static RepositoryCellView Create()
        {
            var cell = new RepositoryCellView();
            var views = NSBundle.MainBundle.LoadNib("RepositoryCellView", cell, null);
            return Runtime.GetNSObject(views.ValueAt(0)) as RepositoryCellView;
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
        }

        public void Set(string name, int stars, int forks, string description, string user, GitHubAvatar avatar, bool showOwner = true)
        {
            CaptionLabel.Text = name;
            FollowersLabel.Text = stars.ToString();
            ForksLabel.Text = forks.ToString();
            ContentLabel.Hidden = string.IsNullOrEmpty(description);
            ContentLabel.Text = description ?? string.Empty;
            UserLabel.Hidden = !showOwner || string.IsNullOrEmpty(user);
            UserImageView.Hidden = UserLabel.Hidden;
            UserLabel.Text = user ?? string.Empty;
            ContentConstraint.Constant = string.IsNullOrEmpty(ContentLabel.Text) ? 0f : DefaultConstraintSize;
            OwnerImageView.SetAvatar(avatar);
        }

        protected override void Dispose(bool disposing)
        {
            ReleaseDesignerOutlets();
            base.Dispose(disposing);
        }
    }
}

