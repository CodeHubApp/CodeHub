using System;
using UIKit;
using Foundation;
using CoreGraphics;

namespace MonoTouch.Dialog
{
	public class ButtonElement: Element
	{
		static NSString Key = new NSString ("ButtonElement");

		public UIColor HighlightedColor, NormalColor, DisabledColor;
		public UIColor HighlightedTextColor, NormalTextColor, DisabledTextColor;

		GlassButton button;
        Action tapped = null;
		
		public ButtonElement (string caption, Action tapped) : base (caption)
		{
			this.tapped = tapped;
			
			NormalColor = UIColor.Gray;
			NormalTextColor = UIColor.Black;

			HighlightedColor = UIColor.Blue;
			HighlightedTextColor = UIColor.White;

			DisabledColor = UIColor.Gray;
			DisabledTextColor = UIColor.DarkGray;
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (Key);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, Key);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
				cell.Frame = new CGRect(cell.Frame.X, cell.Frame.Y, tv.Frame.Width, cell.Frame.Height);
			} else {
				RemoveTag (cell, 1);
			}
			
			if (button == null) {
				
				CGRect frame = cell.Frame;
				frame.Inflate(-10, 0);
				
				button = new GlassButton(frame);
				button.TouchUpInside += (o, e) => tapped.Invoke();
				button.Font = UIFont.BoldSystemFontOfSize (22);
			} else {
				button.RemoveFromSuperview();
			}
			
			button.SetTitle(this.Caption, UIControlState.Normal);
			button.SetTitleColor(UIColor.White, UIControlState.Normal);
			button.BackgroundColor = UIColor.Clear;
			button.HighlightedColor = this.HighlightedColor;
			button.NormalColor = this.NormalColor;
			button.DisabledColor = this.DisabledColor;

			cell.Add(button);

			return cell;
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing && button != null)
			{
				button.RemoveFromSuperview();
				button.Dispose();
				button = null;
			}
		}
		
		public override string Summary ()
		{
			return button.ToString();
		}
	}
}
