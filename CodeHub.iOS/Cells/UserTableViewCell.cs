using System;
using Foundation;
using UIKit;
using CodeHub.Core.ViewModels.Users;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.iOS.Cells
{
    public partial class UserTableViewCell : ReactiveTableViewCell<UserItemViewModel>
    {
        public static readonly UINib Nib = UINib.FromName("UserTableViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("UserTableViewCell");
        public static readonly nfloat DefaultHeight = 58.0f;

        public UserTableViewCell(IntPtr handle)
            : base(handle)
        {
        }

        public static UserTableViewCell Create()
        {
            return (UserTableViewCell)Nib.Instantiate(null, null)[0];
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            SeparatorInset = new UIEdgeInsets(0, UserNameLabel.Frame.X, 0, 0);

            UserImageView.Layer.CornerRadius = UserImageView.Frame.Height / 2f;
            UserImageView.Layer.MasksToBounds = true;
            UserImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
            ContentView.Opaque = true;

            this.OneWayBind(ViewModel, x => x.Name, x => x.UserNameLabel.Text);

            this.WhenAnyValue(x => x.ViewModel)
                .IsNotNull()
                .Select(x => x.Avatar)
                .Subscribe(x => UserImageView.SetAvatar(x));
        }
    }
}

