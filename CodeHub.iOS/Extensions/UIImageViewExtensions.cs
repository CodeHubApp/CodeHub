using System;
using CodeHub.Core.Utilities;
using SDWebImage;
using CodeHub.iOS;
using Foundation;
using CoreAnimation;

// Analysis disable once CheckNamespace
namespace UIKit
{
    public static class UIImageViewExtensions
    {
        public static void SetAvatar(this UIImageView @this, GitHubAvatar avatar, int? size = 64)
        {
            var avatarUri = avatar.ToUri(size);
            if (avatarUri == null)
            {
                @this.Image = Images.LoginUserUnknown;
            }
            else
            {
                @this.SetImage(new NSUrl(avatarUri.AbsoluteUri), Images.LoginUserUnknown, (img, err, type, imageUrl) => {
                    if (img == null || err != null)
                        return;
                    
                    if (type == SDImageCacheType.None)
                    {
                        @this.Image = Images.LoginUserUnknown;
                        @this.BeginInvokeOnMainThread(() =>
                            UIView.Transition(@this, 0.4f, UIViewAnimationOptions.TransitionCrossDissolve, () => @this.Image = img, null));
                    }
                });
            }
        }
    }
}

