using System;
using CodeHub.Core.Utilities;
using SDWebImage;
using CodeHub.iOS;
using Foundation;

// Analysis disable once CheckNamespace
namespace UIKit
{
    public static class UIImageViewExtensions
    {
        public static void SetAvatar(this UIImageView @this, GitHubAvatar avatar, int? size = 64)
        {
            var avatarUri = avatar.ToUri(size);
            if (avatarUri == null)
                @this.Image = Images.LoginUserUnknown;
            else
                @this.SetImage(new NSUrl(avatarUri.AbsoluteUri), Images.LoginUserUnknown);
        }
    }
}

