using UIKit;
using CoreGraphics;
using System;

namespace CodeHub.iOS.Views
{
	public class RetryListView : UIView
	{
		public static UIColor DefaultColor = UIColor.Black;

		public UIImageView ImageView { get; }

		public UILabel Title { get; }

        public UIButton Button { get; }

        public RetryListView(UIImage image, string text, Action retryAction)
			: base(new CGRect(0, 0, 320f, 480f * 2f))
		{
			AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

			ImageView = new UIImageView();
			Title = new UILabel();
            Button = new UIButton(UIButtonType.Custom);
            
			ImageView.Frame = new CGRect(0, 0, 64f, 64f);
			ImageView.TintColor = DefaultColor;
			ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
			ImageView.Image = image;
			Add(ImageView);

			Title.Frame = new CGRect(0, 0, 256f, 20f);
			Title.Text = text;
			Title.TextAlignment = UITextAlignment.Center;
			Title.Font = UIFont.PreferredHeadline;
			Title.TextColor = DefaultColor;
			Add(Title);

            var buttonLabel = new UILabel();
            buttonLabel.Text = "Retry";
            buttonLabel.Font = UIFont.PreferredBody.MakeBold();
            buttonLabel.TextColor = DefaultColor;
            buttonLabel.SizeToFit();
            buttonLabel.AutoresizingMask = UIViewAutoresizing.FlexibleMargins;

            Button.Add(buttonLabel);

            Button.Frame = new CGRect(0, 0, 96, 32);
            Button.Layer.BorderColor = DefaultColor.CGColor;
            Button.Layer.BorderWidth = 1f;
            Button.Layer.CornerRadius = 6f;
            Button.Layer.MasksToBounds = true;
            Button.GetClickedObservable().Subscribe(_ => retryAction());

            Add(Button);

			BackgroundColor = UIColor.White;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			ImageView.Center = new CGPoint(Frame.Width / 2f, (Frame.Height / 2f) - (ImageView.Frame.Height / 2f) - 42);
            Title.Center = new CGPoint(Frame.Width / 2f, ImageView.Frame.Bottom + 30f);
			Button.Center = new CGPoint(Frame.Width / 2f, Title.Frame.Bottom + 40f);
		}
	}
}

