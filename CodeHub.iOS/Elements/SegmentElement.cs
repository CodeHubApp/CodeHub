	
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UIKit;
using Foundation;

namespace MonoTouch.Dialog
{
	public class SegmentElement : Element
	{
		static NSString cpkey = new NSString ("ColorPickerElement");
		UIColor[] colorSelections = null;
		UIImage[] unselectedImages = null;
		UIImage[] selectedImages = null;
		UIColor currentColor = null;
		SegmentControl control;
		
		public delegate void ColorSelectedHandler(UIColor selectedColor);
		public event ColorSelectedHandler ColorSelected;
		
		public delegate void ImageSelectedHandler(int imageIndex);
		public event ImageSelectedHandler ImageSelected;
		
		/// <summary>
		/// Takes an array of UIColor and lays out the colors in a row of selectable items.
		/// </summary>
		public SegmentElement (string caption, UIColor[] colors) : base (caption)
		{
			colorSelections = colors;
		}
		
		/// <summary>
		/// Takes an array of UIImage and lays out the images in a row of selectable items.
		/// A second optional array can also be passed which will be used when an item is selected.
		/// </summary>
		public SegmentElement (string caption, UIImage[] unselectedImages, UIImage[] selectedImages) : base (caption)
		{
			this.unselectedImages = unselectedImages;
			this.selectedImages = selectedImages;
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (cpkey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Subtitle, cpkey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			} else
				RemoveTag (cell, 1);
			
			if (control == null)
			{
				if (colorSelections != null)
				{
					control = new SegmentControl(colorSelections);
					control.ColorSelected += delegate(UIColor selectedColor, UIColor previousColor) {
						if (ColorSelected != null) ColorSelected(selectedColor);
					};
				}
				else if (unselectedImages != null)
				{
					control = new SegmentControl(unselectedImages, selectedImages);
					control.ImageSelected += delegate(int imageIndex) {
						if (ImageSelected != null) ImageSelected(imageIndex);
					};
				}
			}
			else control.SetNeedsDisplay();
			
			cell.AccessoryView = control;
			cell.TextLabel.Text = Caption;
			
			return cell;
		}
		
		public override string Summary ()
		{
			return currentColor.ToString();
		}
	}
}