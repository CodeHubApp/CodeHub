using UIKit;
using CoreGraphics;
using CoreGraphics;
using Foundation;

namespace MonoTouch.Dialog
{
    public class SplitElement : CustomElement
    {
        private static readonly UIFont Font = UIFont.SystemFontOfSize(12f);
        private readonly UIFont _font;

        public Row Value { get; set; }

        public SplitElement(Row row)
            : base(UITableViewCellStyle.Default, "splitelement")
        {
            Value = row;
            BackgroundColor = UIColor.White;
            _font = Font.WithSize(Font.PointSize * Element.FontSizeRatio);
        }

        public class Row
        {
            public string Text1, Text2;
            public UIImage Image1, Image2;
        }


        public override float Height(UITableView tableView, CGRect bounds)
        {
	    return 36f;
        }

		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = base.GetCell(tv);
			cell.SeparatorInset = UIEdgeInsets.Zero;
			return cell;
		}


        public override void Draw(CGRect bounds, CGContext context, UIView view)
        {
            context.BeginPath();
			context.SetLineWidth(1.0f);
			context.SetStrokeColor(UIColor.FromRGB(199, 199, 205).CGColor);
			var x = (int)System.Math.Ceiling(bounds.Width / 2 - 0.5f);
			context.MoveTo(x, 0f);
			context.AddLineToPoint(x, bounds.Height);
            context.StrokePath();

            /*
            context.BeginPath();
            context.SetStrokeColor(UIColor.FromRGBA(250, 250, 250, 128).CGColor);
            context.MoveTo(bounds.Width / 2 + 0.5f, 0);
            context.AddLineToPoint(bounds.Width / 2 + 0.5f, bounds.Height);
            context.StrokePath();
            */

            var row = Value;
            var half = bounds.Height / 2;
            var halfText = _font.LineHeight / 2 + 1;

            row.Image1.Draw(new CGRect(15, half - 8f, 16f, 16f));

            if (!string.IsNullOrEmpty(row.Text1))
            {
                UIColor.Gray.SetColor();
                new NSString(row.Text1).DrawString(new CGRect(36,  half - halfText, bounds.Width / 2 - 40, _font.LineHeight), _font, UILineBreakMode.TailTruncation);
            }

            row.Image2.Draw(new CGRect(bounds.Width / 2 + 15, half - 8f, 16f, 16f));

            if (!string.IsNullOrEmpty(row.Text2))
            {
                UIColor.Gray.SetColor();
                new NSString(row.Text2).DrawString(new CGRect(bounds.Width / 2 + 36,  half - halfText, bounds.Width / 2 - 40, _font.LineHeight), _font, UILineBreakMode.TailTruncation);
            }
        }
    }

}

