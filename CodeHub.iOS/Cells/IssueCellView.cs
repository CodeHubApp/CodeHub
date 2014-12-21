using System;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
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
            AddSubview(new SeperatorIssues {Frame = new RectangleF(65f, 5f, 1f, Frame.Height - 10f)});
            Image1.Image = Theme.CurrentTheme.IssueCellImage1;
            Image2.Image = Theme.CurrentTheme.IssueCellImage2;
            Image3.Image = Theme.CurrentTheme.IssueCellImage3;
            Image4.Image = Theme.CurrentTheme.IssueCellImage4;
            SeparatorInset = new UIEdgeInsets(0, 0, 0, 0);

            Caption.Font = Caption.Font.WithSize(Caption.Font.PointSize * Theme.CurrentTheme.FontSizeRatio);
            Number.Font = Number.Font.WithSize(Number.Font.PointSize * Theme.CurrentTheme.FontSizeRatio);
            Label1.Font = Label1.Font.WithSize(Label1.Font.PointSize * Theme.CurrentTheme.FontSizeRatio);
            Label2.Font = Label2.Font.WithSize(Label2.Font.PointSize * Theme.CurrentTheme.FontSizeRatio);
            Label3.Font = Label3.Font.WithSize(Label3.Font.PointSize * Theme.CurrentTheme.FontSizeRatio);
            Label4.Font = Label4.Font.WithSize(Label4.Font.PointSize * Theme.CurrentTheme.FontSizeRatio);
            IssueType.Font = IssueType.Font.WithSize(IssueType.Font.PointSize * Theme.CurrentTheme.FontSizeRatio);


            this.WhenAnyValue(x => x.ViewModel)
                .Where(x => x != null)
                .Subscribe(x =>
                {
                    Caption.Text = x.Issue.Title;
                    Label1.Text = x.Issue.State;
                    Label2.Text = x.Issue.Comments == 1 ? "1 comment" : x.Issue.Comments + " comments";
                    Label3.Text = x.Issue.Assignee != null ? x.Issue.Assignee.Login : "Unassigned";
                    Label4.Text = x.Issue.UpdatedAt.UtcDateTime.Humanize();
                    Number.Text = "#" + x.Issue.Number;
                    IssueType.Text = x.IsPullRequest ? "Pull" : "Issue";
                });
        }
//
//        static readonly UIFont CountFont = UIFont.BoldSystemFontOfSize (13);
//
//        private class CounterView : UIView
//        {
//            private readonly int _counter;
//            public CounterView(int counter) 
//                : base ()
//            {
//                _counter = counter;
//                BackgroundColor = UIColor.Clear;
//            }
//
//            public override void Draw(RectangleF rect)
//            {
//                if (_counter > 0){
//                    var ctx = UIGraphics.GetCurrentContext ();
//                    var ms = _counter.ToString ();
//
//                    var crect = Bounds;
//                    
//                    UIColor.Gray.SetFill ();
//                    GraphicsUtil.FillRoundedRect (ctx, crect, 3);
//                    UIColor.White.SetColor ();
//                    crect.X += 5;
//                    DrawString (ms, crect, CountFont);
//                }
//                base.Draw(rect);
//            }
//        }

        private class SeperatorIssues : UIView
        {
            public SeperatorIssues()
            {
            }

            public SeperatorIssues(IntPtr handle)
                : base(handle)
            {
            }

            public override void Draw(RectangleF rect)
            {
                base.Draw(rect);

                var context = UIGraphics.GetCurrentContext();
                //context.BeginPath();
                //context.ClipToRect(new RectangleF(63f, 0f, 3f, rect.Height));
                using (var cs = CGColorSpace.CreateDeviceRGB ())
                {
                    using (var gradient = new CGGradient (cs, new [] { 1f, 1f, 1f, 1.0f, 
                        0.7f, 0.7f, 0.7f, 1f, 
                        1f, 1f, 1.0f, 1.0f }, new [] {0, 0.5f, 1f}))
                    {
                        context.DrawLinearGradient(gradient, new PointF(0, 0), new PointF(0, rect.Height), 0);
                    }
                }
            }
        }
    }
}

