using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using CodeHub.Core.ViewModels.App;
using ReactiveUI;
using SDWebImage;
using System.Reactive.Linq;
using Humanizer;

namespace CodeHub.iOS.Cells
{
    public partial class FeedbackCellView : ReactiveTableViewCell<FeedbackItemViewModel>
    {
        public static readonly UINib Nib = UINib.FromName("FeedbackCellView", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("FeedbackCellView");
        private static float DefaultContentConstraintSize = 0.0f;

        public FeedbackCellView(IntPtr handle) 
            : base(handle)
        {
        }

        public static FeedbackCellView Create()
        {
            return Nib.Instantiate(null, null).GetValue(0) as FeedbackCellView;
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            MainImageView.Layer.MasksToBounds = true;
            MainImageView.Layer.CornerRadius = MainImageView.Frame.Height / 2f;
            SeparatorInset = new UIEdgeInsets(0, TitleLabel.Frame.Left, 0, 0);
            TitleLabel.TextColor = Theme.MainTitleColor;
            DetailsLabel.TextColor = UIColor.Gray;
            DefaultContentConstraintSize = DetailsConstraint.Constant;

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    if (string.IsNullOrEmpty(x.ImageUrl))
                        MainImageView.Image = Images.LoginUserUnknown;
                    else
                        MainImageView.SetImage(new NSUrl(x.ImageUrl), Images.LoginUserUnknown);

                    TitleLabel.Text = x.Title;
                    DetailsLabel.Text = "Created " + x.Created.UtcDateTime.Humanize();
                    DetailsConstraint.Constant = string.IsNullOrEmpty(DetailsLabel.Text) ? 0f : DefaultContentConstraintSize;
                });
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            ContentView.SetNeedsLayout();
            ContentView.LayoutIfNeeded();
            TitleLabel.PreferredMaxLayoutWidth = TitleLabel.Frame.Width;
        }
    }
}

