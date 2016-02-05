using System;
using CoreGraphics;
using CoreGraphics;
using UIKit;
using System.Linq;

namespace MonoTouch.Dialog
{
	public class SegmentControl : UIControl
	{
		const float BUTTON_WIDTH = 30;
		
		private UIColor currentColor;
		
		public delegate void ColorSelectedHandler(UIColor selectedColor, UIColor previousColor);
		public event ColorSelectedHandler ColorSelected;
		
		public delegate void ImageSelectedHandler(int imageIndex);
		public event ImageSelectedHandler ImageSelected;
		
		public SegmentControl (UIColor[] colors)
			: base(new CGRect(0, 0, BUTTON_WIDTH * colors.Length, BUTTON_WIDTH))
		{
			_segmentCount = colors.Length;

			for (int i = 0; i < colors.Length; i++)
			{
				ColorView view = new ColorView(new CGRect(BUTTON_WIDTH * i, 0, BUTTON_WIDTH, BUTTON_WIDTH))
					{ BackgroundColor = colors[i] };
				
				view.ColorSelected += delegate(UIColor selectedColor) {
					if (ColorSelected != null)
						ColorSelected(selectedColor, currentColor);
					currentColor = selectedColor;
					foreach (ColorView colorView in Subviews)
					{
						colorView.IsSelected = colorView == view;
						colorView.SetNeedsDisplay();
					}
				};
				
				AddSubview(view);
			}
		}
		
		public SegmentControl (UIImage[] unselectedImages, UIImage[] selectedImages)
			: base(new CGRect(0, 0, BUTTON_WIDTH * unselectedImages.Length, BUTTON_WIDTH))
		{
			SetBitmapItems(unselectedImages, selectedImages);
		}

		public void SetBitmapItems(UIImage[] unselectedImages, UIImage[] selectedImages)
		{
			_segmentCount = unselectedImages.Length;
			
			// set the new frame size
			Frame = new CGRect(Frame.Left, Frame.Top, BUTTON_WIDTH * unselectedImages.Length, BUTTON_WIDTH);
			
			// kill the old views
			foreach (var view in Subviews)
				view.RemoveFromSuperview();
			
			// add the new views	
			for (int ii = 0; ii < unselectedImages.Length; ii++)
			{
				UIButton view = new UIButton(new CGRect(BUTTON_WIDTH * ii, 0, BUTTON_WIDTH, BUTTON_WIDTH))
					{ BackgroundColor = UIColor.White };

				view.SetImage(unselectedImages[ii], UIControlState.Normal);
				
				if (selectedImages != null && selectedImages.Length > ii)
					view.SetImage(selectedImages[ii], UIControlState.Selected);

				//var index = ii;
				view.TouchUpInside += delegate(object sender, EventArgs e) {
					handleSelection(sender as UIButton);
				};	
				AddSubview(view);
			}
		}
		
		private void handleSelection(UIButton sender)
		{
			int newSelectedIndex = -1;
			for (int buttonIndex = 0; buttonIndex < Subviews.Length; buttonIndex++) {
				UIButton button = Subviews[buttonIndex] as UIButton;
				if (button == sender)
				{
					if (!button.Selected) 
					{
						newSelectedIndex = buttonIndex;
						button.Selected = true;
					}
					else
						button.Selected = false;
				} 
				else
					button.Selected = false;
			}
			if (ImageSelected != null) {
				_selected = newSelectedIndex;
				ImageSelected(newSelectedIndex);
			}
		}
		
		public int Count
		{
			get { return _segmentCount; }
		}
		int _segmentCount = 0;
		
		public new int Selected
		{
			get { return _selected; }
		}
		int _selected = -1;
		
		public void SetSelectedImage(int index)
		{
			for (int i = 0; i < Subviews.Length; i++)
			{
				if (Subviews[i] is UIButton)
				{
					((UIButton)Subviews[i]).Selected = (i == index);
				}
			}
			_selected = index;
		}
		
		public override void Draw (CGRect rect)
		{
			base.Draw (rect);
			
			foreach (UIControl control in Subviews)
				control.SetNeedsDisplay();
		}
		
		private class ColorView : UIControl
		{
			public bool IsSelected { get; set; }
				
			public delegate void ColorSelectEventHandler(UIColor selectedColor);
			public event ColorSelectEventHandler ColorSelected;
			
			public ColorView(CGRect frame) : base(frame)
			{
				AddTarget(delegate { ColorSelected(BackgroundColor); }, UIControlEvent.TouchUpInside);
			}
			
			public override void Draw (CGRect rect)
			{
				base.Draw (rect);

				if (!IsSelected)
					return;
				
				CGContext context = UIGraphics.GetCurrentContext();
				context.SaveState();
                new UIColor(0, 0, 0, 0.5f).SetFill();
				context.BeginPath();
				
				// top border
				context.AddRect(new CGRect(0, 0, Frame.Width, 4));
				
				// left border
				context.AddRect(new CGRect(0, 0, 4, Frame.Height));
				
				// right border
				context.AddRect(new CGRect(Frame.Width - 4, 0, 4, Frame.Height));
				
				// bottom border
				context.AddRect(new CGRect(0, Frame.Height - 4, Frame.Width, 4));
				
				context.ClosePath();
				context.FillPath();
	
				context.RestoreState();
				context.SetBlendMode(CoreGraphics.CGBlendMode.Clear);
			}
		}
	}
}

