using UIKit;
using CodeHub.Core.Utilities;

namespace CodeHub.iOS.DialogElements
{
    public class UserElement : StringElement
    {
        public UserElement(string username, string name, GitHubAvatar avatar)
            : base (username, string.Empty, UITableViewCellStyle.Subtitle)
        {
             if (!string.IsNullOrWhiteSpace(name))
                Value = name;
            Accessory = UITableViewCellAccessory.DisclosureIndicator;
            Image = Images.Avatar;
            ImageUri = avatar.ToUri(64);
        }
        
        // We need to create our own cell so we can position the image view appropriately
        protected override UITableViewCell CreateTableViewCell(UITableViewCellStyle style, string key)
        {
            return new PinnedImageTableViewCell(style, key);
        }

        /// <summary>
        /// This class is to make sure the imageview is of a specific size... :(
        /// </summary>
        private class PinnedImageTableViewCell : UITableViewCell
        {
            public PinnedImageTableViewCell(UITableViewCellStyle style, string key) 
                : base(style, key) 
            {
                SeparatorInset = new UIEdgeInsets(0, 48f, 0, 0);

                ImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
                ImageView.Layer.CornerRadius = 16f;
                ImageView.Layer.MasksToBounds = true;
//                ImageView.Layer.ShouldRasterize = true;
//                ImageView.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
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

