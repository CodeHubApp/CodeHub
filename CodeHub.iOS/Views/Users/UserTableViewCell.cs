using System;
using CodeHub.Core.ViewModels.Users;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.UIKit;
using ReactiveUI;
using System.Reactive.Linq;
using SDWebImage;

namespace CodeHub.iOS.Views.Users
{
    public class UserTableViewCell : ReactiveTableViewCell<UserItemViewModel>
    {
        public static NSString Key = new NSString("usercell");
        private const float ImageSpacing = 8f;
        private static RectangleF ImageFrame = new RectangleF(ImageSpacing, 6f, 32, 32);

        public UserTableViewCell(IntPtr handle)
            : base(handle)
        { 
            SeparatorInset = new UIEdgeInsets(0, ImageFrame.Right + ImageSpacing, 0, 0);
            ImageView.Layer.CornerRadius = ImageFrame.Height / 2f;
            ImageView.Layer.MasksToBounds = true;
            ImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            ContentView.Opaque = true;

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    TextLabel.Text = x.Name;
                    ImageView.SetImage(url: new NSUrl(x.Url), placeholder: Images.LoginUserUnknown);
                });
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            ImageView.Frame = ImageFrame;
            TextLabel.Frame = new RectangleF(ImageFrame.Right + ImageSpacing, TextLabel.Frame.Y, TextLabel.Frame.Width, TextLabel.Frame.Height);
        }
    }
}

