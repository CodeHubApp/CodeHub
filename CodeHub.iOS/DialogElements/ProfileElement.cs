using UIKit;
using CodeHub.Core.Utilities;

namespace CodeHub.iOS.DialogElements
{
    public class ProfileElement : StringElement
    {
        public ProfileElement(string username, string name, GitHubAvatar avatar)
            : base (username, string.Empty, UITableViewCellStyle.Subtitle)
        {
             if (!string.IsNullOrWhiteSpace(name))
                Value = name;
            Accessory = UITableViewCellAccessory.DisclosureIndicator;
            Image = Images.Avatar;
            ImageUri = avatar.ToUri(64);
        }

        public static ProfileElement FromUser(Octokit.User user)
            => new ProfileElement(user.Login, user.Name, new GitHubAvatar(user.AvatarUrl));

        public static ProfileElement FromOrganization(Octokit.Organization org)
            => new ProfileElement(org.Login, org.Name, new GitHubAvatar(org.AvatarUrl));
        
        protected override UITableViewCell CreateTableViewCell(UITableViewCellStyle style, string key)
            => new PinnedImageTableViewCell(style, key);

        private class PinnedImageTableViewCell : UITableViewCell
        {
            public PinnedImageTableViewCell(UITableViewCellStyle style, string key) 
                : base(style, key) 
            {
                SeparatorInset = new UIEdgeInsets(0, 48f, 0, 0);

                ImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
                ImageView.Layer.CornerRadius = 16f;
                ImageView.Layer.MasksToBounds = true;
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();
                ImageView.Frame = new CoreGraphics.CGRect(6, 6, 32, 32);
                TextLabel.Frame = new CoreGraphics.CGRect(48, TextLabel.Frame.Y, TextLabel.Frame.Width, TextLabel.Frame.Height);
                if (DetailTextLabel != null)
                    DetailTextLabel.Frame = new CoreGraphics.CGRect(48, DetailTextLabel.Frame.Y, DetailTextLabel.Frame.Width, DetailTextLabel.Frame.Height);
            }
        }
    }
}

