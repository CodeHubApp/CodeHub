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

            Image1.Image = Images.Gear.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            Image1.TintColor = Label1.TextColor;

            Image2.Image = Images.Comment.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            Image2.TintColor = Label2.TextColor;

            Image3.Image = Images.Person.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            Image3.TintColor = Label3.TextColor;

            Image4.Image = Images.Pencil.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
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
                .Subscribe(x =>
                {
                    Caption.Text = x.Title;
                    Label1.Text = x.State;
                    Label2.Text = x.Comments == 1 ? "1 comment" : x.Comments + " comments";
                    Label3.Text = x.Assignee;
                    Label4.Text = x.UpdatedAt.LocalDateTime.Humanize();
                    Number.Text = "#" + x.Number;
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

