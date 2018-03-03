using System;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;
using Humanizer;

namespace CodeHub.iOS.TableViewCells
{
    public partial class IssueCellView : UITableViewCell
    {
        public static readonly NSString Key = new NSString("IssueCellView");

        public static IssueCellView Create()
        {
            var cell = new IssueCellView();
            var views = NSBundle.MainBundle.LoadNib("IssueCellView", cell, null);
            cell = Runtime.GetNSObject( views.ValueAt(0) ) as IssueCellView;

            if (cell == null)
            {
                Console.WriteLine("Null cell!");
            }
            else
            {
                cell.Caption.TextColor = Theme.CurrentTheme.MainTitleColor;
                cell.Number.TextColor = Theme.CurrentTheme.MainTitleColor;
                cell.ContentView.AddSubview(new SeperatorIssues {Frame = new CGRect(65f, 5f, 1f, cell.Frame.Height - 10f)});
                cell.Image1.Image = Octicon.Gear.ToImage(12);
                cell.Image2.Image = Octicon.CommentDiscussion.ToImage(12);
                cell.Image3.Image = Octicon.Person.ToImage(12);
                cell.Image4.Image = Octicon.Pencil.ToImage(12);
                cell.SeparatorInset = new UIEdgeInsets(0, 0, 0, 0);
            }

            return cell;
        }

        public override NSString ReuseIdentifier
        {
            get
            {
                return Key;
            }
        }

        public IssueCellView()
        {
        }

        public IssueCellView(IntPtr handle)
            : base(handle)
        {
        }

        public void Bind(string title, string status, string priority, string assigned, DateTimeOffset lastUpdated, string id, string kind)
        {
            Caption.Text = title;
            Label1.Text = status;
            Label2.Text = priority;
            Label3.Text = assigned;
            Label4.Text = lastUpdated.UtcDateTime.Humanize();
            Number.Text = "#" + id;
            IssueType.Text = kind;
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
                using (var cs = CGColorSpace.CreateDeviceRGB ())
                {
                    using (var gradient = new CGGradient (cs, new nfloat [] { 1f, 1f, 1f, 1.0f, 
                        0.7f, 0.7f, 0.7f, 1f, 
                        1f, 1f, 1.0f, 1.0f }, new nfloat [] {0, 0.5f, 1f}))
                    {
                        context.DrawLinearGradient(gradient, new CGPoint(0, 0), new CGPoint(0, rect.Height), 0);
                    }
                }
            }
        }
    }
}

