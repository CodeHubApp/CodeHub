using System;
using UIKit;
using CodeHub.iOS;

namespace CodeHub.iOS.DialogElements
{
    public class UserElement : StringElement
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CodeHub.iOS.Elements.UserElement"/> use pinned image.
        /// </summary>
        /// <value><c>true</c> if use pinned image; otherwise, <c>false</c>.</value>
        public bool UsePinnedImage { get; set; }

        public UserElement(string username, string firstName, string lastName, string avatar)
            : base (username, string.Empty, UITableViewCellStyle.Subtitle)
        {
            var realName = firstName ?? "" + " " + (lastName ?? "");
             if (!string.IsNullOrWhiteSpace(realName))
                Value = realName;
            Accessory = UITableViewCellAccessory.DisclosureIndicator;
            Image = Images.Avatar;
            if (avatar != null)
                ImageUri = new Uri(avatar);
            UsePinnedImage = true;
        }
        
        // We need to create our own cell so we can position the image view appropriately
        protected override UITableViewCell CreateTableViewCell(UITableViewCellStyle style, string key)
        {
            if (UsePinnedImage)
                return new PinnedImageTableViewCell(style, key);
            else
                return base.CreateTableViewCell(style, key);
        }

        /// <summary>
        /// This class is to make sure the imageview is of a specific size... :(
        /// </summary>
        private class PinnedImageTableViewCell : UITableViewCell
        {
            public PinnedImageTableViewCell(UITableViewCellStyle style, string key) 
                : base(style, key) 
            { 
                this.SeparatorInset = new UIKit.UIEdgeInsets(0, 48f, 0, 0); 
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

