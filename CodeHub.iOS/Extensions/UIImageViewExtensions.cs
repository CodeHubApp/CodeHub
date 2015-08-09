using System;
using CodeHub.Core.Utilities;
using CodeHub.iOS;
using SDWebImage;
using Foundation;

// Analysis disable once CheckNamespace
namespace UIKit
{
    public static class UIImageViewExtensions
    {
        public static void SetAvatar(this UIImageView @this, GitHubAvatar avatar, int? size = 64)
        {
            @this.Image = Images.UnknownUser;

            if (avatar == null)
                return;

            var avatarUri = avatar.ToUri(size);
            if (avatarUri != null)
            {
                @this.SetImage(new NSUrl(avatarUri.AbsoluteUri), Images.LoginUserUnknown, (img, err, type, imageUrl) => {
                    if (img == null || err != null)
                        return;
                    
                    if (type == SDImageCacheType.None)
                    {
                        @this.Image = Images.UnknownUser;
                        @this.BeginInvokeOnMainThread(() =>
                            UIView.Transition(@this, 0.25f, UIViewAnimationOptions.TransitionCrossDissolve, () => @this.Image = img, null));
                    }
                });
            }
        }
    }
}

