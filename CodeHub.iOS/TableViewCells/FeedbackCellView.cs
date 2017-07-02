using System;
using Foundation;
using UIKit;
using CodeHub.Core.ViewModels.App;
using ReactiveUI;
using System.Reactive.Linq;
using SDWebImage;

namespace CodeHub.iOS.TableViewCells
{
    public partial class FeedbackCellView : ReactiveTableViewCell<FeedbackItemViewModel>
    {
        public static readonly UINib Nib = UINib.FromName("FeedbackCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("FeedbackCellView");
        private static nfloat DefaultContentConstraintSize = 0.0f;

        public FeedbackCellView(IntPtr handle) 
            : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            MainImageView.Layer.MasksToBounds = true;
            MainImageView.Layer.CornerRadius = MainImageView.Frame.Height / 2f;
            ContentView.Opaque = true;

            SeparatorInset = new UIEdgeInsets(0, TitleLabel.Frame.Left, 0, 0);
            TitleLabel.TextColor = Theme.CurrentTheme.MainTitleColor;
            DetailsLabel.TextColor = UIColor.Gray;
            DefaultContentConstraintSize = DetailsConstraint.Constant;

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x => {
	                TitleLabel.Text = x.Title;
	                DetailsLabel.Text = "Created " + x.CreatedString;
	                DetailsConstraint.Constant = string.IsNullOrEmpty(DetailsLabel.Text) ? 0f : DefaultContentConstraintSize;

	                if (string.IsNullOrEmpty(x.ImageUrl))
	                    MainImageView.Image = Images.LoginUserUnknown;
	                else
	                    MainImageView.SetImage(new NSUrl(x.ImageUrl), Images.LoginUserUnknown);
	            });
        }
    }
}

