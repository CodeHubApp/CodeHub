using System;
using Foundation;
using UIKit;
using CodeHub.Core.ViewModels.Users;
using ReactiveUI;
using CoreGraphics;

namespace CodeHub.iOS.TableViewCells
{
	public class UserTableViewCell : ReactiveTableViewCell<UserItemViewModel>
	{
		public static readonly NSString Key = new NSString("UserTableViewCell");
		private const float ImageSpacing = 10f;
        private static readonly CGRect ImageFrame = new CGRect(ImageSpacing, 6f, 32, 32);

        public UserTableViewCell(IntPtr handle)
			: base(handle)
		{
			ImageView.Layer.CornerRadius = ImageFrame.Height / 2f;
			ImageView.Layer.MasksToBounds = true;
			ImageView.ContentMode = UIViewContentMode.ScaleAspectFill;
			ContentView.Opaque = true;

            SeparatorInset = new UIEdgeInsets(0, ImageFrame.Right + ImageSpacing, 0, 0);

			this.WhenActivated(d =>
			{
                d(this.WhenAnyValue(x => x.ViewModel.Login).Subscribe(x => TextLabel.Text = x));
				d(this.WhenAnyValue(x => x.ViewModel.Avatar).Subscribe(x => ImageView.SetAvatar(x)));
			});
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			ImageView.Frame = ImageFrame;
			TextLabel.Frame = new CGRect(ImageFrame.Right + ImageSpacing, TextLabel.Frame.Y,
                                         TextLabel.Frame.Width, TextLabel.Frame.Height);
		}
	}
}

