using System;
using CoreGraphics;
using Foundation;
using UIKit;
using CodeHub.iOS;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.iOS.Cells;
using ReactiveUI;
using System.Reactive.Linq;
using Humanizer;

namespace CodeHub.iOS.Cells
{
    public partial class IssueCellView : ReactiveTableViewCell<IssueItemViewModel>
    {
        public static readonly UINib Nib = UINib.FromName("IssueCellView", NSBundle.MainBundle);
        public static NSString Key = new NSString("IssueCellView");

        public IssueCellView(IntPtr handle) 
            : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            Caption.TextColor = Theme.MainTitleColor;
            Number.TextColor = Theme.MainTitleColor;
            AddSubview(new SeperatorIssues {Frame = new CGRect(65f, 5f, 1f, Frame.Height - 10f)});

            Image1.Image = Octicon.Gear.ToImage(Image1.Frame.Height);
            Image1.TintColor = Label1.TextColor;

            Image2.Image = Octicon.Comment.ToImage(Image2.Frame.Height);
            Image2.TintColor = Label2.TextColor;

            Image3.Image = Octicon.Person.ToImage(Image3.Frame.Height);
            Image3.TintColor = Label3.TextColor;

            Image4.Image = Octicon.Pencil.ToImage(Image4.Frame.Height);
            Image4.TintColor = Label4.TextColor;

            SeparatorInset = new UIEdgeInsets(0, 0, 0, 0);

            const float fontRatio = 1.0f;
            Caption.Font = Caption.Font.WithSize(Caption.Font.PointSize * fontRatio);
            Number.Font = Number.Font.WithSize(Number.Font.PointSize * fontRatio);
            Label1.Font = Label1.Font.WithSize(Label1.Font.PointSize * fontRatio);
            Label2.Font = Label2.Font.WithSize(Label2.Font.PointSize * fontRatio);
            Label3.Font = Label3.Font.WithSize(Label3.Font.PointSize * fontRatio);
            Label4.Font = Label4.Font.WithSize(Label4.Font.PointSize * fontRatio);
            IssueType.Font = IssueType.Font.WithSize(IssueType.Font.PointSize * fontRatio);

            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x => {
                    Caption.Text = x.Title;
                    Label1.Text = x.State;
                    Label2.Text = x.Comments == 1 ? "1 comment" : x.Comments + " comments";
                    Label3.Text = x.Assignee;
                    Label4.Text = x.UpdatedAt.Humanize();
                    Number.Text = "#" + x.Number;
                    IssueType.Text = x.IsPullRequest ? "Pull" : "Issue";
                });
        }

        private class SeperatorIssues : UIView
        {
            public SeperatorIssues()
            {
            }

            public SeperatorIssues(IntPtr handle)
                : base(handle)
            {
            }

            public override void Draw(CGRect rect)
            {
                base.Draw(rect);

                var context = UIGraphics.GetCurrentContext();
                //context.BeginPath();
                //context.ClipToRect(new RectangleF(63f, 0f, 3f, rect.Height));
                using (var cs = CGColorSpace.CreateDeviceRGB ())
                {
                    using (var gradient = new CGGradient (cs, new nfloat[] { 1f, 1f, 1f, 1.0f, 
                        0.7f, 0.7f, 0.7f, 1f, 
                        1f, 1f, 1.0f, 1.0f }, new nfloat[] {0, 0.5f, 1f}))
                    {
                        context.DrawLinearGradient(gradient, new CGPoint(0, 0), new CGPoint(0, rect.Height), 0);
                    }
                }
            }
        }
    }
}

