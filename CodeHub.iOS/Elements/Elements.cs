
//
// Elements.cs: defines the various components of our view
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// Copyright 2010, Novell, Inc.
//
// Code licensed under the MIT X11 license
//
// TODO: StyledStringElement: merge with multi-line?
// TODO: StyledStringElement: add image scaling features?
// TODO: StyledStringElement: add sizing based on image size?
// TODO: Move image rendering to StyledImageElement, reason to do this: the image loader would only be imported in this case, linked out otherwise
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UIKit;
using CoreGraphics;
using CoreGraphics;
using Foundation;
using CoreAnimation;
using SDWebImage;

namespace MonoTouch.Dialog
{
	/// <summary>
	/// Base class for all elements in MonoTouch.Dialog
	/// </summary>
	public class Element : IDisposable {
        public static float FontSizeRatio = 1.0f;

		/// <summary>
		///  Handle to the container object.
		/// </summary>
		/// <remarks>
		/// For sections this points to a RootElement, for every
		/// other object this points to a Section and it is null
		/// for the root RootElement.
		/// </remarks>
		public Element Parent;
		
		/// <summary>
		///  The caption to display for this given element
		/// </summary>
		public string Caption;

		/// <summary>
		///  Initializes the element with the given caption.
		/// </summary>
		/// <param name="caption">
		/// The caption.
		/// </param>
		public Element (string caption)
		{
			this.Caption = caption;
		}	

        public Element() : this (string.Empty) {}
		
		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
		}
		
		static NSString cellkey = new NSString ("xx");
		/// <summary>
		/// Subclasses that override the GetCell method should override this method as well
		/// </summary>
		/// <remarks>
		/// This method should return the key passed to UITableView.DequeueReusableCell.
		/// If your code overrides the GetCell method to change the cell, you must also 
		/// override this method and return a unique key for it.
		/// 
		/// This works in most subclasses with a couple of exceptions: StringElement and
		/// various derived classes do not use this setting as they need a wider range
		/// of keys for different uses, so you need to look at the source code for those
		/// if you are trying to override StringElement or StyledStringElement.
		/// </remarks>
		protected virtual NSString CellKey { 
			get {
				return cellkey;
			}
		}
		
		/// <summary>
		/// Gets a UITableViewCell for this element.   Can be overridden, but if you 
		/// customize the style or contents of the cell you must also override the CellKey 
		/// property in your derived class.
		/// </summary>
		public virtual UITableViewCell GetCell (UITableView tv)
		{
			return new UITableViewCell (UITableViewCellStyle.Default, CellKey);
		}
		
		static protected void RemoveTag (UITableViewCell cell, int tag)
		{
			var viewToRemove = cell.ContentView.ViewWithTag (tag);
			if (viewToRemove != null)
				viewToRemove.RemoveFromSuperview ();
		}
		
		/// <summary>
		/// Returns a summary of the value represented by this object, suitable 
		/// for rendering as the result of a RootElement with child objects.
		/// </summary>
		/// <returns>
		/// The return value must be a short description of the value.
		/// </returns>
		public virtual string Summary ()
		{
			return "";
		}
		
		/// <summary>
		/// Invoked when the given element has been deslected by the user.
		/// </summary>
		/// <param name="dvc">
		/// The <see cref="DialogViewController"/> where the deselection took place
		/// </param>
		/// <param name="tableView">
		/// The <see cref="UITableView"/> that contains the element.
		/// </param>
		/// <param name="path">
		/// The <see cref="NSIndexPath"/> that contains the Section and Row for the element.
		/// </param>
		public virtual void Deselected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
		}
		
		/// <summary>
		/// Invoked when the given element has been selected by the user.
		/// </summary>
		/// <param name="dvc">
		/// The <see cref="DialogViewController"/> where the selection took place
		/// </param>
		/// <param name="tableView">
		/// The <see cref="UITableView"/> that contains the element.
		/// </param>
		/// <param name="path">
		/// The <see cref="NSIndexPath"/> that contains the Section and Row for the element.
		/// </param>
		public virtual void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
		}

		/// <summary>
		/// If the cell is attached will return the immediate RootElement
		/// </summary>
		public RootElement GetImmediateRootElement ()
		{
				var section = Parent as Section;
                if (section == null)
                    section = this as Section;
				if (section == null)
					return null;
				return section.Parent as RootElement;
		}
		
		/// <summary>
		/// Returns the UITableView associated with this element, or null if this cell is not currently attached to a UITableView
		/// </summary>
		public UITableView GetContainerTableView ()
		{
			var root = GetImmediateRootElement ();
			if (root == null)
				return null;
			return root.TableView;
		}
		
		/// <summary>
		/// Returns the currently active UITableViewCell for this element, or null if the element is not currently visible
		/// </summary>
		public UITableViewCell GetActiveCell ()
		{
			var tv = GetContainerTableView ();
			if (tv == null)
				return null;
			var path = IndexPath;
			if (path == null)
				return null;
			return tv.CellAt (path);
		}
		
		/// <summary>
		///  Returns the IndexPath of a given element.   This is only valid for leaf elements,
		///  it does not work for a toplevel RootElement or a Section of if the Element has
		///  not been attached yet.
		/// </summary>
		public NSIndexPath IndexPath { 
			get {
				var section = Parent as Section;
				if (section == null)
					return null;
				var root = section.Parent as RootElement;
				if (root == null)
					return null;
				
				int row = 0;
				foreach (var element in section.Elements){
					if (element == this){
						int nsect = 0;
						foreach (var sect in root.Sections){
							if (section == sect){
								return NSIndexPath.FromRowSection (row, nsect);
							}
							nsect++;
						}
					}
					row++;
				}
				return null;
			}
		}
		
		/// <summary>
		///   Method invoked to determine if the cell matches the given text, never invoked with a null value or an empty string.
		/// </summary>
		public virtual bool Matches (string text)
		{
			if (Caption == null)
				return false;
			return Caption.IndexOf (text, StringComparison.CurrentCultureIgnoreCase) != -1;
		}
	}

	public abstract class BoolElement : Element {
		bool val;
		public virtual bool Value {
			get {
				return val;
			}
			set {
				bool emit = val != value;
				val = value;
				if (emit && ValueChanged != null)
					ValueChanged (this, EventArgs.Empty);
			}
		}
		public event EventHandler ValueChanged;
		
		public BoolElement (string caption, bool value) : base (caption)
		{
			val = value;
		}
		
		public override string Summary ()
		{
			return val ? "On".GetText () : "Off".GetText ();
		}		
	}
	
	/// <summary>
	/// Used to display switch on the screen.
	/// </summary>
	public class BooleanElement : BoolElement {
		static NSString bkey = new NSString ("BooleanElement");
		UISwitch sw;
		
		public BooleanElement (string caption, bool value) : base (caption, value)
		{  }
		
		public BooleanElement (string caption, bool value, string key) : base (caption, value)
		{  }
		
		protected override NSString CellKey {
			get {
				return bkey;
			}
		}
		public override UITableViewCell GetCell (UITableView tv)
		{
			if (sw == null){
				sw = new UISwitch (){
					BackgroundColor = UIColor.Clear,
					Tag = 1,
					On = Value
				};
				sw.AddTarget (delegate {
					Value = sw.On;
				}, UIControlEvent.ValueChanged);
			} else
				sw.On = Value;
			
			var cell = tv.DequeueReusableCell (CellKey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, CellKey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			} else
				RemoveTag (cell, 1);
		
			cell.TextLabel.Text = Caption;
			cell.AccessoryView = sw;

			return cell;
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing){
				if (sw != null){
					sw.Dispose ();
					sw = null;
				}
			}
		}
		
		public override bool Value {
			get {
				return base.Value;
			}
			set {
				 base.Value = value;
				if (sw != null)
					sw.On = value;
			}
		}
	}
	
	/// <summary>
	///  This class is used to render a string + a state in the form
	/// of an image.  
	/// </summary>
	/// <remarks>
	/// It is abstract to avoid making this element
	/// keep two pointers for the state images, saving 8 bytes per
	/// slot.   The more derived class "BooleanImageElement" shows
	/// one way to implement this by keeping two pointers, a better
	/// implementation would return pointers to images that were 
	/// preloaded and are static.
	/// 
	/// A subclass only needs to implement the GetImage method.
	/// </remarks>
	public abstract class BaseBooleanImageElement : BoolElement {
		static NSString key = new NSString ("BooleanImageElement");
		
		public class TextWithImageCellView : UITableViewCell {
			const int fontSize = 17;
			static UIFont font = UIFont.BoldSystemFontOfSize (fontSize);
			BaseBooleanImageElement parent;
			UILabel label;
			UIButton button;
			const int ImageSpace = 32;
			const int Padding = 8;
	
			public TextWithImageCellView (BaseBooleanImageElement parent_) : base (UITableViewCellStyle.Value1, parent_.CellKey)
			{
				parent = parent_;
				label = new UILabel () {
					TextAlignment = UITextAlignment.Left,
					Text = parent.Caption,
                    Font = font.WithSize(font.PointSize * Element.FontSizeRatio),
					BackgroundColor = UIColor.Clear
				};
				button = UIButton.FromType (UIButtonType.Custom);
				button.TouchDown += delegate {
					parent.Value = !parent.Value;
					UpdateImage ();
					if (parent.Tapped != null)
						parent.Tapped ();
				};
				ContentView.Add (label);
				ContentView.Add (button);
				UpdateImage ();
			}

			void UpdateImage ()
			{
				button.SetImage (parent.GetImage (), UIControlState.Normal);
			}
			
			public override void LayoutSubviews ()
			{
				base.LayoutSubviews ();
				var full = ContentView.Bounds;
				var frame = full;
				frame.Height = 22;
				frame.X = Padding;
				frame.Y = (full.Height-frame.Height)/2;
				frame.Width -= ImageSpace+Padding;
				label.Frame = frame;
				
				button.Frame = new CGRect (full.Width-ImageSpace, -3, ImageSpace, 48);
			}
			
			public void UpdateFrom (BaseBooleanImageElement newParent)
			{
				parent = newParent;
				UpdateImage ();
				label.Text = parent.Caption;
				SetNeedsDisplay ();
			}
		}
	
		public BaseBooleanImageElement (string caption, bool value)
			: base (caption, value)
		{
		}
		
        public event Action Tapped;
		
		protected abstract UIImage GetImage ();
		
		protected override NSString CellKey {
			get {
				return key;
			}
		}
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (CellKey) as TextWithImageCellView;
			if (cell == null)
				cell = new TextWithImageCellView (this);
			else
				cell.UpdateFrom (this);
			return cell;
		}
	}
	
	public class BooleanImageElement : BaseBooleanImageElement {
		UIImage onImage, offImage;
		
		public BooleanImageElement (string caption, bool value, UIImage onImage, UIImage offImage) : base (caption, value)
		{
			this.onImage = onImage;
			this.offImage = offImage;
		}
		
		protected override UIImage GetImage ()
		{
			if (Value)
				return onImage;
			else
				return offImage;
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			onImage = offImage = null;
		}
	}
	
	/// <summary>
	///  Used to display a slider on the screen.
	/// </summary>
	public class FloatElement : Element {
		public bool ShowCaption;
		public float Value;
		public float MinValue, MaxValue;
		static NSString skey = new NSString ("FloatElement");
		UISlider slider;
		
		public FloatElement (float value) : base (null)
		{
			MinValue = 0;
			MaxValue = 1;
			Value = value;
		}
		
		protected override NSString CellKey {
			get {
				return skey;
			}
		}
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (CellKey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, CellKey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			} else
				RemoveTag (cell, 1);

			CGSize captionSize = new CGSize (0, 0);
			if (Caption != null && ShowCaption){
				cell.TextLabel.Text = Caption;
                captionSize = new NSString(Caption).StringSize (UIFont.FromName (cell.TextLabel.Font.Name, UIFont.LabelFontSize));
				captionSize.Width += 10; // Spacing
			}

			if (slider == null){
				slider = new UISlider (new CGRect (10f + captionSize.Width, 12f, 280f - captionSize.Width, 7f)){
					BackgroundColor = UIColor.Clear,
					MinValue = this.MinValue,
					MaxValue = this.MaxValue,
					Continuous = true,
					Value = this.Value,
					Tag = 1
				};
				slider.ValueChanged += delegate {
					Value = slider.Value;
				};
			} else {
				slider.Value = Value;
			}
			
			cell.ContentView.AddSubview (slider);
			return cell;
		}

		public override string Summary ()
		{
			return Value.ToString ();
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing){
				if (slider != null){
					slider.Dispose ();
					slider = null;
				}
			}
		}		
	}

	/// <summary>
	///   The string element can be used to render some text in a cell 
	///   that can optionally respond to tap events.
	/// </summary>
	public class StringElement : Element {
		static NSString skey = new NSString ("StringElement");
		static NSString skeyvalue = new NSString ("StringElementValue");
		public UITextAlignment Alignment = UITextAlignment.Left;
		public string Value;
		
		public StringElement (string caption) : base (caption) {}
		
		public StringElement (string caption, string value) : base (caption)
		{
			this.Value = value;
		}
		
		public StringElement (string caption,  Action tapped) : base (caption)
		{
			Tapped += tapped;
		}
		
        public event Action Tapped;
				
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (Value == null ? skey : skeyvalue);
			if (cell == null){
				cell = new UITableViewCell (Value == null ? UITableViewCellStyle.Default : UITableViewCellStyle.Value1, skey);
				cell.SelectionStyle = (Tapped != null) ? UITableViewCellSelectionStyle.Blue : UITableViewCellSelectionStyle.None;
			}
			cell.Accessory = UITableViewCellAccessory.None;
			cell.TextLabel.Text = Caption;
			cell.TextLabel.TextAlignment = Alignment;
			
			// The check is needed because the cell might have been recycled.
			if (cell.DetailTextLabel != null)
				cell.DetailTextLabel.Text = Value == null ? "" : Value;
			
			return cell;
		}

		public override string Summary ()
		{
			return Caption;
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			if (Tapped != null)
				Tapped ();
			tableView.DeselectRow (indexPath, true);
		}
		
		public override bool Matches (string text)
		{
			return (Value != null ? Value.IndexOf (text, StringComparison.CurrentCultureIgnoreCase) != -1: false) || base.Matches (text);
		}
	}
	
	/// <summary>
	///   A version of the StringElement that can be styled with a number of formatting 
	///   options and can render images or background images either from UIImage parameters 
	///   or by downloading them from the net.
	/// </summary>
	public class StyledStringElement : StringElement, IColorizeBackground {
        public static UIFont  DefaultTitleFont = UIFont.SystemFontOfSize(15f);
        public static UIFont  DefaultDetailFont = UIFont.SystemFontOfSize(14f);
        public static UIColor DefaultTitleColor = UIColor.FromRGB(41, 41, 41);
        public static UIColor DefaultDetailColor = UIColor.FromRGB(120, 120, 120);
		public static UIColor BgColor = UIColor.White;

		static NSString [] skey = { new NSString (".1"), new NSString (".2"), new NSString (".3"), new NSString (".4") };
		
		public StyledStringElement (string caption) : base (caption) 
        {
            Init();
        }

        public StyledStringElement (string caption, Action tapped) : base (caption, tapped) 
        {
            Accessory = UITableViewCellAccessory.DisclosureIndicator;
            Init();
        }

        public StyledStringElement (string caption, Action tapped, UIImage image) : base (caption, tapped) 
        {
            Accessory = UITableViewCellAccessory.DisclosureIndicator;
            Init();
            Image = image;
        }


		public StyledStringElement (string caption, string value) : base (caption, value) 
		{
			style = UITableViewCellStyle.Value1;
            Init();
		}
		public StyledStringElement (string caption, string value, UITableViewCellStyle style) : base (caption, value) 
		{ 
			this.style = style;
            Init();
		}
		
		protected UITableViewCellStyle style;
        public event Action AccessoryTapped;
		public UIFont Font;
		public UIFont SubtitleFont;
		public UIColor TextColor;
		public UILineBreakMode LineBreakMode = UILineBreakMode.WordWrap;
		public int Lines = 1;
		public UITableViewCellAccessory Accessory = UITableViewCellAccessory.None;

        private void Init()
        {
            Font = DefaultTitleFont.WithSize(DefaultTitleFont.PointSize * Element.FontSizeRatio);
            SubtitleFont = DefaultDetailFont.WithSize(DefaultDetailFont.PointSize * Element.FontSizeRatio);
            BackgroundColor = BgColor;
            TextColor = DefaultTitleColor;
            DetailColor = DefaultDetailColor;
            LineBreakMode = UILineBreakMode.TailTruncation;
        }
		
		// To keep the size down for a StyleStringElement, we put all the image information
		// on a separate structure, and create this on demand.
		ExtraInfo extraInfo;
		
		class ExtraInfo {
			public UIImage Image; // Maybe add BackgroundImage?
			public UIColor BackgroundColor, DetailColor;
			public Uri Uri;
		}

		ExtraInfo OnImageInfo ()
		{
			if (extraInfo == null)
				extraInfo = new ExtraInfo ();
			return extraInfo;
		}
		
		// Uses the specified image (use this or ImageUri)
		public UIImage Image {
			get {
				return extraInfo == null ? null : extraInfo.Image;
			}
			set {
				OnImageInfo ().Image = value;
			}
		}
		
		// Loads the image from the specified uri (use this or Image)
		public Uri ImageUri {
			get {
				return extraInfo == null ? null : extraInfo.Uri;
			}
			set {
				OnImageInfo ().Uri = value;
			}
		}
		
		// Background color for the cell (alternative: BackgroundUri)
		public UIColor BackgroundColor {
			get {
				return extraInfo == null ? null : extraInfo.BackgroundColor;
			}
			set {
				OnImageInfo ().BackgroundColor = value;
			}
		}
		
		public UIColor DetailColor {
			get {
				return extraInfo == null ? null : extraInfo.DetailColor;
			}
			set {
				OnImageInfo ().DetailColor = value;
			}
		}

		protected virtual string GetKey (int style)
		{
			return skey [style];
		}

        protected virtual void OnCellCreated(UITableViewCell cell)
        {
        }

        protected virtual UITableViewCell CreateTableViewCell(UITableViewCellStyle style, string key)
        {
            return new UITableViewCell (style, key);
        }
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var key = GetKey ((int) style);
			var cell = tv.DequeueReusableCell (key);
			if (cell == null){
                cell = CreateTableViewCell(style, key);
                OnCellCreated(cell);
				cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
			}
			PrepareCell (cell);
			return cell;
		}
		
		protected void PrepareCell (UITableViewCell cell)
		{
			cell.Accessory = Accessory;
			var tl = cell.TextLabel;
			tl.Text = Caption;
			tl.TextAlignment = Alignment;
			tl.TextColor = TextColor ?? UIColor.Black;
			tl.Font = Font ?? UIFont.BoldSystemFontOfSize (17);
			tl.LineBreakMode = LineBreakMode;
			tl.Lines = Lines;	
			
			// The check is needed because the cell might have been recycled.
			if (cell.DetailTextLabel != null)
				cell.DetailTextLabel.Text = Value == null ? "" : Value;
			
			if (extraInfo == null){
				ClearBackground (cell);
			} else {
                cell.ImageView.Image = extraInfo.Image;

                if (extraInfo.Uri != null)
                {
                    cell.ImageView.SetImage(new NSUrl(extraInfo.Uri.AbsoluteUri), extraInfo.Image);
                }
	
				if (cell.DetailTextLabel != null)
					cell.DetailTextLabel.TextColor = extraInfo.DetailColor ?? UIColor.Gray;
			}
				
			if (cell.DetailTextLabel != null){
				cell.DetailTextLabel.Lines = Lines;
				cell.DetailTextLabel.LineBreakMode = LineBreakMode;
				cell.DetailTextLabel.Font = SubtitleFont ?? UIFont.SystemFontOfSize (14);
				cell.DetailTextLabel.TextColor = (extraInfo == null || extraInfo.DetailColor == null) ? UIColor.Gray : extraInfo.DetailColor;
			}
		}	
	
		void ClearBackground (UITableViewCell cell)
		{
			cell.BackgroundColor = UIColor.White;
            cell.TextLabel.BackgroundColor = UIColor.Clear;

            if (cell.DetailTextLabel != null)
                cell.DetailTextLabel.BackgroundColor = UIColor.Clear;
		}

		void IColorizeBackground.WillDisplay (UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
		{
			ClearBackground (cell);

            if (extraInfo == null)
                return;
			
			if (extraInfo.BackgroundColor != null){
				cell.BackgroundColor = extraInfo.BackgroundColor;
				cell.TextLabel.BackgroundColor = UIColor.Clear;
			}
		}

		internal void AccessoryTap ()
		{
            var tapped = AccessoryTapped;
			if (tapped != null)
				tapped ();
		}
	}
	
	public class StyledMultilineElement : StyledStringElement, IElementSizing {
		public StyledMultilineElement (string caption) : base (caption) {}
		public StyledMultilineElement (string caption, string value) : base (caption, value) {}
        public StyledMultilineElement (string caption, Action tapped) : base (caption, tapped) {}
		public StyledMultilineElement (string caption, string value, UITableViewCellStyle style) : base (caption, value) 
		{ 
			this.style = style;
		}

		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = base.GetCell (tv);				
			var tl = cell.TextLabel;
			tl.LineBreakMode = UILineBreakMode.WordWrap;
			tl.Lines = 0;

			var sl = cell.DetailTextLabel;
			if (sl != null) {
				sl.LineBreakMode = UILineBreakMode.WordWrap;
				sl.Lines = 0;
			}

			return cell;
		}

		public virtual nfloat GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			const float margin = 30f;
			CGSize maxSize = new CGSize (tableView.Bounds.Width - margin, float.MaxValue);
			
			if (this.Accessory != UITableViewCellAccessory.None)
				maxSize.Width -= 20;
			
			string c = Caption;
			string v = Value;
			// ensure the (multi-line) Value will be rendered inside the cell when no Caption is present
			if (String.IsNullOrEmpty (c) && !String.IsNullOrEmpty (v))
				c = " ";

			var captionFont = Font ?? UIFont.BoldSystemFontOfSize (17);
            nfloat height = new NSString(c).StringSize (captionFont, maxSize, LineBreakMode).Height;
			
			if (!String.IsNullOrEmpty (v)) {
				var subtitleFont = SubtitleFont ?? UIFont.SystemFontOfSize (14);
				if (this.style == UITableViewCellStyle.Subtitle) {
                    height += new NSString(v).StringSize (subtitleFont, maxSize, LineBreakMode).Height;
				} else {
                    var vheight = new NSString(v).StringSize (subtitleFont, maxSize, LineBreakMode).Height;
					if (vheight > height)
						height = vheight;
				}
			}
			
			return height + 10;
		}
	}
	
	public class ImageStringElement : StringElement {
		static NSString skey = new NSString ("ImageStringElement");
		UIImage image;
		public UITableViewCellAccessory Accessory { get; set; }
		
		public ImageStringElement (string caption, UIImage image) : base (caption)
		{
			this.image = image;
			this.Accessory = UITableViewCellAccessory.None;
		}

		public ImageStringElement (string caption, string value, UIImage image) : base (caption, value)
		{
			this.image = image;
			this.Accessory = UITableViewCellAccessory.None;
		}
		
        public ImageStringElement (string caption,  Action tapped, UIImage image) : base (caption, tapped)
		{
			this.image = image;
			this.Accessory = UITableViewCellAccessory.None;
		}
		
		protected override NSString CellKey {
			get {
				return skey;
			}
		}
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (CellKey);
			if (cell == null){
				cell = new UITableViewCell (Value == null ? UITableViewCellStyle.Default : UITableViewCellStyle.Subtitle, CellKey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
			}
			
			cell.Accessory = Accessory;
			cell.TextLabel.Text = Caption;
			cell.TextLabel.TextAlignment = Alignment;
			
			cell.ImageView.Image = image;
			
			// The check is needed because the cell might have been recycled.
			if (cell.DetailTextLabel != null)
				cell.DetailTextLabel.Text = Value == null ? "" : Value;
			
			return cell;
		}
		
	}
	
	/// <summary>
	///   This interface is implemented by Element classes that will have
	///   different heights
	/// </summary>
	public interface IElementSizing {
		nfloat GetHeight (UITableView tableView, NSIndexPath indexPath);
	}
	
	/// <summary>
	///   This interface is implemented by Elements that needs to update
	///   their cells Background properties just before they are displayed
	///   to the user.   This is an iOS 3 requirement to properly render
	///   a cell.
	/// </summary>
	public interface IColorizeBackground {
		void WillDisplay (UITableView tableView, UITableViewCell cell, NSIndexPath indexPath);
	}
	
	public class RadioElement : StringElement {
		public string Group;
		internal int RadioIdx;
		
		public RadioElement (string caption, string group) : base (caption)
		{
			Group = group;
		}
				
		public RadioElement (string caption) : base (caption)
		{
		}

		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = base.GetCell (tv);			
			var root = (RootElement) Parent.Parent;
			
			if (!(root.group is RadioGroup))
				throw new Exception ("The RootElement's Group is null or is not a RadioGroup");
			
			bool selected = RadioIdx == ((RadioGroup)(root.group)).Selected;
			cell.Accessory = selected ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;

			return cell;
		}

		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			RootElement root = (RootElement) Parent.Parent;
			if (RadioIdx != root.RadioSelected){
				UITableViewCell cell;
				var selectedIndex = root.PathForRadio (root.RadioSelected);
				if (selectedIndex != null) {
					cell = tableView.CellAt (selectedIndex);
					if (cell != null)
						cell.Accessory = UITableViewCellAccessory.None;
				}				
				cell = tableView.CellAt (indexPath);
				if (cell != null)
					cell.Accessory = UITableViewCellAccessory.Checkmark;
				root.RadioSelected = RadioIdx;
			}
			
			base.Selected (dvc, tableView, indexPath);
		}
	}
	
	public class CheckboxElement : StringElement {
		public new bool Value;
		public string Group;
		
		public CheckboxElement (string caption) : base (caption) {}
		public CheckboxElement (string caption, bool value) : base (caption)
		{
			Value = value;
		}
		
		public CheckboxElement (string caption, bool value, string group) : this (caption, value)
		{
			Group = group;
		}
		
		UITableViewCell ConfigCell (UITableViewCell cell)
		{
			cell.Accessory = Value ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
			return cell;
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			return  ConfigCell (base.GetCell (tv));
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			Value = !Value;
			var cell = tableView.CellAt (path);
			ConfigCell (cell);
			base.Selected (dvc, tableView, path);
		}

	}
	
	public class ImageElement : Element {
		public UIImage Value;
		static CGRect rect = new CGRect (0, 0, dimx, dimy);
		static NSString ikey = new NSString ("ImageElement");
		UIImage scaled;
		UIPopoverController popover;
		
		// Apple leaks this one, so share across all.
		static UIImagePickerController picker;
		
		// Height for rows
		const int dimx = 48;
		const int dimy = 43;
		
		// radius for rounding
		const int rad = 10;
		
		static UIImage MakeEmpty ()
		{
			using (var cs = CGColorSpace.CreateDeviceRGB ()){
				using (var bit = new CGBitmapContext (IntPtr.Zero, dimx, dimy, 8, 0, cs, CGImageAlphaInfo.PremultipliedFirst)){
					bit.SetStrokeColor (1, 0, 0, 0.5f);
					bit.FillRect (new CGRect (0, 0, dimx, dimy));
					
					return UIImage.FromImage (bit.ToImage ());
				}
			}
		}
		
		UIImage Scale (UIImage source)
		{
			UIGraphics.BeginImageContext (new CGSize (dimx, dimy));
			var ctx = UIGraphics.GetCurrentContext ();
		
			var img = source.CGImage;
			ctx.TranslateCTM (0, dimy);
			if (img.Width > img.Height)
				ctx.ScaleCTM (1, -img.Width/dimy);
			else
				ctx.ScaleCTM (img.Height/dimx, -1);

			ctx.DrawImage (rect, source.CGImage);
			
			var ret = UIGraphics.GetImageFromCurrentImageContext ();
			UIGraphics.EndImageContext ();
			return ret;
		}
		
		public ImageElement (UIImage image) : base ("")
		{
			if (image == null){
				Value = MakeEmpty ();
				scaled = Value;
			} else {
				Value = image;			
				scaled = Scale (Value);
			}
		}
		
		protected override NSString CellKey {
			get {
				return ikey;
			}
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (CellKey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, CellKey);
			}
			
			if (scaled == null)
				return cell;
			
			Section psection = Parent as Section;
			bool roundTop = psection.Elements [0] == this;
			bool roundBottom = psection.Elements [psection.Elements.Count-1] == this;
			
			using (var cs = CGColorSpace.CreateDeviceRGB ()){
				using (var bit = new CGBitmapContext (IntPtr.Zero, dimx, dimy, 8, 0, cs, CGImageAlphaInfo.PremultipliedFirst)){
					// Clipping path for the image, different on top, middle and bottom.
					if (roundBottom){
						bit.AddArc (rad, rad, rad, (float) Math.PI, (float) (3*Math.PI/2), false);
					} else {
						bit.MoveTo (0, rad);
						bit.AddLineToPoint (0, 0);
					}
					bit.AddLineToPoint (dimx, 0);
					bit.AddLineToPoint (dimx, dimy);
					
					if (roundTop){
						bit.AddArc (rad, dimy-rad, rad, (float) (Math.PI/2), (float) Math.PI, false);
						bit.AddLineToPoint (0, rad);
					} else {
						bit.AddLineToPoint (0, dimy);
					}
					bit.Clip ();
					bit.DrawImage (rect, scaled.CGImage);
					
					cell.ImageView.Image = UIImage.FromImage (bit.ToImage ());
				}
			}			
			return cell;
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing){
				if (scaled != null){
					scaled.Dispose ();
					Value.Dispose ();
					scaled = null;
					Value = null;
				}
			}
			base.Dispose (disposing);
		}

		class MyDelegate : UIImagePickerControllerDelegate {
			ImageElement container;
			UITableView table;
			NSIndexPath path;
			
			public MyDelegate (ImageElement container, UITableView table, NSIndexPath path)
			{
				this.container = container;
				this.table = table;
				this.path = path;
			}
			
			public override void FinishedPickingImage (UIImagePickerController picker, UIImage image, NSDictionary editingInfo)
			{
				container.Picked (image);
				table.ReloadRows (new NSIndexPath [] { path }, UITableViewRowAnimation.None);
			}
		}
		
		void Picked (UIImage image)
		{
			Value = image;
			scaled = Scale (image);
			currentController.DismissViewController(true, null);
			
		}
		
		UIViewController currentController;
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			if (picker == null)
				picker = new UIImagePickerController ();
			picker.Delegate = new MyDelegate (this, tableView, path);
			
			switch (UIDevice.CurrentDevice.UserInterfaceIdiom){
			case UIUserInterfaceIdiom.Pad:
				popover = new UIPopoverController (picker);
				var cell = tableView.CellAt (path);
				if (cell != null)
					rect = cell.Frame;
				popover.PresentFromRect (rect, dvc.View, UIPopoverArrowDirection.Any, true);
				break;
				
			default:
			case UIUserInterfaceIdiom.Phone:
				dvc.ActivateController (picker);
				break;
			}
			currentController = dvc;
		}
	}
	
	/// <summary>
	/// An element that can be used to enter text.
	/// </summary>
	/// <remarks>
	/// This element can be used to enter text both regular and password protected entries. 
	///     
	/// The Text fields in a given section are aligned with each other.
	/// </remarks>
	public class EntryElement : Element {
		/// <summary>
		///   The value of the EntryElement
		/// </summary>
		public string Value { 
			get {
				if (entry == null)
					return val;
				var newValue = entry.Text;
				if (newValue == val)
					return val;
				val = newValue;

				if (Changed != null)
					Changed (this, EventArgs.Empty);
				return val;
			}
			set {
				val = value;
				if (entry != null)
					entry.Text = value;
			}
		}
		protected string val;

		/// <summary>
		/// The key used for reusable UITableViewCells.
		/// </summary>
		static NSString entryKey = new NSString ("EntryElement");
		protected virtual NSString EntryKey {
			get {
				return entryKey;
			}
		}

		/// <summary>
		/// The type of keyboard used for input, you can change
		/// this to use this for numeric input, email addressed,
		/// urls, phones.
		/// </summary>
		public UIKeyboardType KeyboardType {
			get {
				return keyboardType;
			}
			set {
				keyboardType = value;
				if (entry != null)
					entry.KeyboardType = value;
			}
		}
		
		/// <summary>
		/// The type of Return Key that is displayed on the
		/// keyboard, you can change this to use this for
		/// Done, Return, Save, etc. keys on the keyboard
		/// </summary>
		public UIReturnKeyType? ReturnKeyType {
			get {
				return returnKeyType;
			}
			set {
				returnKeyType = value;
				if (entry != null && returnKeyType.HasValue)
					entry.ReturnKeyType = returnKeyType.Value;
			}
		}
		
		public UITextAutocapitalizationType AutocapitalizationType {
			get {
				return autocapitalizationType;	
			}
			set { 
				autocapitalizationType = value;
				if (entry != null)
					entry.AutocapitalizationType = value;
			}
		}
		
		public UITextAutocorrectionType AutocorrectionType { 
			get { 
				return autocorrectionType;
			}
			set { 
				autocorrectionType = value;
				if (entry != null)
					this.autocorrectionType = value;
			}
		}
		
		public UITextFieldViewMode ClearButtonMode { 
			get { 
				return clearButtonMode;
			}
			set { 
				clearButtonMode = value;
				if (entry != null)
					entry.ClearButtonMode = value;
			}
		}

		public UITextAlignment TextAlignment {
			get {
				return textalignment;
			}
			set{
				textalignment = value;
				if (entry != null) {
					entry.TextAlignment = textalignment;
				}
			}
		}
		UITextAlignment textalignment = UITextAlignment.Left;
		UIKeyboardType keyboardType = UIKeyboardType.Default;
		UIReturnKeyType? returnKeyType = null;
		UITextAutocapitalizationType autocapitalizationType = UITextAutocapitalizationType.Sentences;
		UITextAutocorrectionType autocorrectionType = UITextAutocorrectionType.Default;
		UITextFieldViewMode clearButtonMode = UITextFieldViewMode.Never;
		bool isPassword, becomeResponder;
		UITextField entry;
		string placeholder;

		public event EventHandler Changed;
		public event Func<bool> ShouldReturn;
		public EventHandler EntryStarted {get;set;}
		public EventHandler EntryEnded {get;set;}
        public UIFont TitleFont { get; set; }
        public UIFont EntryFont { get; set; }
        public UIColor TitleColor { get; set; }

		/// <summary>
		/// Constructs an EntryElement with the given caption, placeholder and initial value.
		/// </summary>
		/// <param name="caption">
		/// The caption to use
		/// </param>
		/// <param name="placeholder">
		/// Placeholder to display when no value is set.
		/// </param>
		/// <param name="value">
		/// Initial value.
		/// </param>
		public EntryElement (string caption, string placeholder, string value) : base (caption)
		{ 
            TitleFont = UIFont.BoldSystemFontOfSize (17 * Element.FontSizeRatio);
            EntryFont = UIFont.SystemFontOfSize(17 * Element.FontSizeRatio);
            TitleColor = UIColor.Black;
			Value = value;
			this.placeholder = placeholder;
		}
		
		/// <summary>
		/// Constructs an EntryElement for password entry with the given caption, placeholder and initial value.
		/// </summary>
		/// <param name="caption">
		/// The caption to use.
		/// </param>
		/// <param name="placeholder">
		/// Placeholder to display when no value is set.
		/// </param>
		/// <param name="value">
		/// Initial value.
		/// </param>
		/// <param name="isPassword">
		/// True if this should be used to enter a password.
		/// </param>
		public EntryElement (string caption, string placeholder, string value, bool isPassword) : base (caption)
		{
            TitleFont = UIFont.BoldSystemFontOfSize (17 * Element.FontSizeRatio);
            EntryFont = UIFont.SystemFontOfSize(17 * Element.FontSizeRatio);
            TitleColor = UIColor.Black;
			Value = value;
			this.isPassword = isPassword;
			this.placeholder = placeholder;
		}

		public override string Summary ()
		{
			return Value;
		}

		// 
		// Computes the X position for the entry by aligning all the entries in the Section
		//
		CGSize ComputeEntryPosition (UITableView tv, UITableViewCell cell)
		{
			Section s = Parent as Section;
			if (s.EntryAlignment.Width != 0)
				return s.EntryAlignment;
			
			// If all EntryElements have a null Caption, align UITextField with the Caption
			// offset of normal cells (at 10px).
            CGSize max = new CGSize (-15, new NSString("M").StringSize (TitleFont).Height);
			foreach (var e in s.Elements){
				var ee = e as EntryElement;
				if (ee == null)
					continue;
				
				if (ee.Caption != null) {
                    var size = new NSString(ee.Caption).StringSize (TitleFont);
					if (size.Width > max.Width)
						max = size;
				}
			}
			s.EntryAlignment = new CGSize (25 + Math.Min (max.Width, 160), max.Height);
			return s.EntryAlignment;
		}

		protected virtual UITextField CreateTextField (CGRect frame)
		{
			return new UITextField (frame) {
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleLeftMargin,
				Placeholder = placeholder ?? "",
				SecureTextEntry = isPassword,
				Text = Value ?? "",
				Tag = 1,
				TextAlignment = textalignment,
				ClearButtonMode = ClearButtonMode,
                Font = EntryFont,
			};
		}
		
		static NSString cellkey = new NSString ("EntryElement");
		
		protected override NSString CellKey {
			get {
				return cellkey;
			}
		}

		UITableViewCell cell;
		public override UITableViewCell GetCell (UITableView tv)
		{
			if (cell == null) {
				cell = new UITableViewCell (UITableViewCellStyle.Default, CellKey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;

			} 
			cell.TextLabel.Text = Caption;

			var offset = (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone) ? 20 : 90;
			cell.Frame = new CGRect(cell.Frame.X, cell.Frame.Y, tv.Frame.Width-offset, cell.Frame.Height);
			CGSize size = ComputeEntryPosition (tv, cell);
			nfloat yOffset = (cell.ContentView.Bounds.Height - size.Height) / 2 - 1;
			nfloat width = cell.ContentView.Bounds.Width - size.Width;
			if (textalignment == UITextAlignment.Right) {
				// Add padding if right aligned
				width -= 10;
			}
            var entryFrame = new CGRect (size.Width, yOffset + 2f, width, size.Height);

			if (entry == null) {
				entry = CreateTextField (entryFrame);
				entry.ValueChanged += delegate {
					FetchValue ();
				};
				entry.Ended += delegate {                                        
					FetchValue ();
					if (EntryEnded != null) {
						EntryEnded (this, null);
					}
				};
				entry.ShouldReturn += delegate {

					if (ShouldReturn != null)
						return ShouldReturn ();

					RootElement root = GetImmediateRootElement ();
					EntryElement focus = null;

					if (root == null)
						return true;

					foreach (var s in root.Sections) {
						foreach (var e in s.Elements) {
							if (e == this) {
								focus = this;
							} else if (focus != null && e is EntryElement) {
								focus = e as EntryElement;
								break;
							}
						}

						if (focus != null && focus != this)
							break;
					}

					if (focus != this)
						focus.BecomeFirstResponder (true);
					else 
						focus.ResignFirstResponder (true);

					return true;
				};
				entry.Started += delegate {
					EntryElement self = null;

					if (EntryStarted != null) {
						EntryStarted (this, null);
					}

					if (!returnKeyType.HasValue) {
						var returnType = UIReturnKeyType.Default;

						foreach (var e in (Parent as Section).Elements) {
							if (e == this)
								self = this;
							else if (self != null && e is EntryElement)
								returnType = UIReturnKeyType.Next;
						}
						entry.ReturnKeyType = returnType;
					} else
						entry.ReturnKeyType = returnKeyType.Value;

					tv.ScrollToRow (IndexPath, UITableViewScrollPosition.Middle, true);
				};
				cell.ContentView.AddSubview (entry);
			} else
				entry.Frame = entryFrame;

			if (becomeResponder){
				entry.BecomeFirstResponder ();
				becomeResponder = false;
			}
			entry.KeyboardType = KeyboardType;

			entry.AutocapitalizationType = AutocapitalizationType;
			entry.AutocorrectionType = AutocorrectionType;
			cell.TextLabel.Text = Caption;
            cell.TextLabel.Font = TitleFont;
            cell.TextLabel.TextColor = TitleColor;

			return cell;
		}
		
		/// <summary>
		///  Copies the value from the UITextField in the EntryElement to the
		//   Value property and raises the Changed event if necessary.
		/// </summary>
		public void FetchValue ()
		{
			if (entry == null)
				return;

			var newValue = entry.Text;
			if (newValue == Value)
				return;
			
			Value = newValue;
			
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing){
				if (entry != null){
					entry.Dispose ();
					entry = null;
				}

				if (cell != null) {
					cell.Dispose ();
					cell = null;
				}
			}
		}

		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			BecomeFirstResponder(true);
			tableView.DeselectRow (indexPath, true);
		}
		
		public override bool Matches (string text)
		{
			return (Value != null ? Value.IndexOf (text, StringComparison.CurrentCultureIgnoreCase) != -1: false) || base.Matches (text);
		}
		
		/// <summary>
		/// Makes this cell the first responder (get the focus)
		/// </summary>
		/// <param name="animated">
		/// Whether scrolling to the location of this cell should be animated
		/// </param>
		public virtual void BecomeFirstResponder (bool animated)
		{
			becomeResponder = true;
			var tv = GetContainerTableView ();
			if (tv == null)
				return;
			tv.ScrollToRow (IndexPath, UITableViewScrollPosition.Middle, animated);
			if (entry != null){
				entry.BecomeFirstResponder ();
				becomeResponder = false;
			}
		}

		public virtual void ResignFirstResponder (bool animated)
		{
			becomeResponder = false;
			var tv = GetContainerTableView ();
			if (tv == null)
				return;
			tv.ScrollToRow (IndexPath, UITableViewScrollPosition.Middle, animated);
			if (entry != null)
				entry.ResignFirstResponder ();
		}
	}
	
	/// <summary>
	///   This element can be used to insert an arbitrary UIView
	/// </summary>
	/// <remarks>
	///   There is no cell reuse here as we have a 1:1 mapping
	///   in this case from the UIViewElement to the cell that
	///   holds our view.
	/// </remarks>
	public class UIViewElement : Element, IElementSizing {
		static int count;
		public UIView ContainerView;
		NSString key;
		protected UIView View;
		public CellFlags Flags;
		UIEdgeInsets insets;

		public UIEdgeInsets Insets { 
			get {
				return insets;
			}
			set {
				var viewFrame = View.Frame;
				var dx = value.Left - insets.Left;
				var dy = value.Top - insets.Top;
				var ow = insets.Left + insets.Right;
				var oh = insets.Top + insets.Bottom;
				var w = value.Left + value.Right;
				var h = value.Top + value.Bottom;

				ContainerView.Frame = new CGRect (0, 0, ContainerView.Frame.Width + w - ow, ContainerView.Frame.Height + h -oh);
				viewFrame.X += dx;
				viewFrame.Y += dy;
				View.Frame = viewFrame;

				insets = value;

				// Height changed, notify UITableView
				if (dy != 0 || h != oh)
					GetContainerTableView ().ReloadData ();
				
			}
		}

		public enum CellFlags {
			Transparent = 1,
			DisableSelection = 2
		}


		/// <summary>
		///   Constructor
		/// </summary>
		/// <param name="caption">
		/// The caption, only used for RootElements that might want to summarize results
		/// </param>
		/// <param name="view">
		/// The view to display
		/// </param>
		/// <param name="transparent">
		/// If this is set, then the view is responsible for painting the entire area,
		/// otherwise the default cell paint code will be used.
		/// </param>
		public UIViewElement (string caption, UIView view, bool transparent, UIEdgeInsets insets) : base (caption) 
		{
			this.insets = insets;
			var oframe = view.Frame;
			var frame = oframe;
			frame.Width += insets.Left + insets.Right;
			frame.Height += insets.Top + insets.Bottom;

			ContainerView = new UIView (frame);
			if ((Flags & CellFlags.Transparent) != 0)
				ContainerView.BackgroundColor = UIColor.Clear;

			if (insets.Left != 0 || insets.Top != 0)
				view.Frame = new CGRect (insets.Left + frame.X, insets.Top + frame.Y, frame.Width, frame.Height);

			ContainerView.AddSubview (view);
			this.View = view;
			this.Flags = transparent ? CellFlags.Transparent : 0;
			key = new NSString ("UIViewElement" + count++);
		}
		
		public UIViewElement (string caption, UIView view, bool transparent) : this (caption, view, transparent, UIEdgeInsets.Zero)
		{
		}

		protected override NSString CellKey {
			get {
				return key;
			}
		}
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (CellKey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, CellKey);
				if ((Flags & CellFlags.Transparent) != 0){
					cell.BackgroundColor = UIColor.Clear;
					
					// 
					// This trick is necessary to keep the background clear, otherwise
					// it gets painted as black
					//
					cell.BackgroundView = new UIView (CGRect.Empty) { 
						BackgroundColor = UIColor.Clear 
					};
				}
				if ((Flags & CellFlags.DisableSelection) != 0)
					cell.SelectionStyle = UITableViewCellSelectionStyle.None;

				if (Caption != null)
					cell.TextLabel.Text = Caption;
				cell.ContentView.AddSubview (ContainerView);
			} 
			return cell;
		}
		
		public nfloat GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			return ContainerView.Bounds.Height+1;
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if (disposing){
				if (View != null){
					View.Dispose ();
					View = null;
				}
			}
		}
	}
	
	/// <summary>
	/// Sections contain individual Element instances that are rendered by MonoTouch.Dialog
	/// </summary>
	/// <remarks>
	/// Sections are used to group elements in the screen and they are the
	/// only valid direct child of the RootElement.    Sections can contain
	/// any of the standard elements, including new RootElements.
	/// 
	/// RootElements embedded in a section are used to navigate to a new
	/// deeper level.
	/// 
	/// You can assign a header and a footer either as strings (Header and Footer)
	/// properties, or as UIViews to be shown (HeaderView and FooterView).   Internally
	/// this uses the same storage, so you can only show one or the other.
	/// </remarks>
	public class Section : Element, IEnumerable {
		object header, footer;
        public readonly List<Element> Elements = new List<Element> ();
				
		// X corresponds to the alignment, Y to the height of the password
		public CGSize EntryAlignment;
		
		/// <summary>
		///  Constructs a Section without header or footers.
		/// </summary>
		public Section () : base (null) {}
		
		/// <summary>
		///  Constructs a Section with the specified header
		/// </summary>
		/// <param name="caption">
		/// The header to display
		/// </param>
		public Section (string caption) : base (caption)
		{
		}
		
		/// <summary>
		/// Constructs a Section with a header and a footer
		/// </summary>
		/// <param name="caption">
		/// The caption to display (or null to not display a caption)
		/// </param>
		/// <param name="footer">
		/// The footer to display.
		/// </param>
		public Section (string caption, string footer) : base (caption)
		{
			Footer = footer;
		}

		public Section (UIView header) : base (null)
		{
			HeaderView = header;
		}
		
		public Section (UIView header, UIView footer) : base (null)
		{
			HeaderView = header;
			FooterView = footer;
		}
		
		/// <summary>
		///    The section header, as a string
		/// </summary>
		public string Header {
			get {
				return header as string;
			}
			set {
				header = value;
			}
		}
		
		/// <summary>
		/// The section footer, as a string.
		/// </summary>
		public string Footer {
			get {
				return footer as string;
			}
			
			set {
				footer = value;
			}
		}
		
		/// <summary>
		/// The section's header view.  
		/// </summary>
		public UIView HeaderView {
			get {
				return header as UIView;
			}
			set {
				header = value;
			}
		}
		
		/// <summary>
		/// The section's footer view.
		/// </summary>
		public UIView FooterView {
			get {
				return footer as UIView;
			}
			set {
				footer = value;
			}
		}
		
		/// <summary>
		/// Adds a new child Element to the Section
		/// </summary>
		/// <param name="element">
		/// An element to add to the section.
		/// </param>
		public void Add (Element element)
		{
			if (element == null)
				return;
			
			Elements.Add (element);
			element.Parent = this;
			
			if (Parent != null)
				InsertVisual (Elements.Count-1, UITableViewRowAnimation.None, 1);
		}

		/// <summary>
		/// Adds a new child RootElement to the Section. This only exists to fix a compiler breakage when the mono 3.0 mcs is used.
		/// </summary>
		/// <param name="element">
		/// An element to add to the section.
		/// </param>
		public void Add (RootElement element)
		{
			Add ((Element)element);
		}

		/// <summary>
		///    Add version that can be used with LINQ
		/// </summary>
		/// <param name="elements">
		/// An enumerable list that can be produced by something like:
		///    from x in ... select (Element) new MyElement (...)
		/// </param>
		public int AddAll (IEnumerable<Element> elements)
		{
			int count = 0;
			foreach (var e in elements){
				Add (e);
				count++;
			}
			return count;
		}
		
		/// <summary>
		///    This method is being obsoleted, use AddAll to add an IEnumerable<Element> instead.
		/// </summary>
		[Obsolete ("Please use AddAll since this version will not work in future versions of MonoTouch when we introduce 4.0 covariance")]
		public int Add (IEnumerable<Element> elements)
		{
			return AddAll (elements);
		}
		
		/// <summary>
		/// Use to add a UIView to a section, it makes the section opaque, to
		/// get a transparent one, you must manually call UIViewElement
		public void Add (UIView view)
		{
			if (view == null)
				return;
			Add (new UIViewElement (null, view, false));
		}

		/// <summary>
		///   Adds the UIViews to the section.
		/// </summary>
		/// <param name="views">
		/// An enumarable list that can be produced by something like:
		///    from x in ... select (UIView) new UIFoo ();
		/// </param>
		public void Add (IEnumerable<UIView> views)
		{
			foreach (var v in views)
				Add (v);
		}
		
		/// <summary>
		/// Inserts a series of elements into the Section using the specified animation
		/// </summary>
		/// <param name="idx">
		/// The index where the elements are inserted
		/// </param>
		/// <param name="anim">
		/// The animation to use
		/// </param>
		/// <param name="newElements">
		/// A series of elements.
		/// </param>
		public void Insert (int idx, UITableViewRowAnimation anim, params Element [] newElements)
		{
			if (newElements == null)
				return;
			
			int pos = idx;
			foreach (var e in newElements){
				Elements.Insert (pos++, e);
				e.Parent = this;
			}
			var root = Parent as RootElement;
			if (Parent != null && root.TableView != null){
				if (anim == UITableViewRowAnimation.None)
					root.TableView.ReloadData ();
				else
					InsertVisual (idx, anim, newElements.Length);
			}
		}

		public int Insert (int idx, UITableViewRowAnimation anim, IEnumerable<Element> newElements)
		{
			if (newElements == null)
				return 0;

			int pos = idx;
			int count = 0;
			foreach (var e in newElements){
				Elements.Insert (pos++, e);
				e.Parent = this;
				count++;
			}
			var root = Parent as RootElement;
			if (root != null && root.TableView != null){				
				if (anim == UITableViewRowAnimation.None)
					root.TableView.ReloadData ();
				else
					InsertVisual (idx, anim, pos-idx);
			}
			return count;
		}
		
		/// <summary>
		/// Inserts a single RootElement into the Section using the specified animation
		/// </summary>
		/// <param name="idx">
		/// The index where the elements are inserted
		/// </param>
		/// <param name="anim">
		/// The animation to use
		/// </param>
		/// <param name="newElements">
		/// A series of elements.
		/// </param>
		public void Insert (int idx, UITableViewRowAnimation anim, RootElement newElement)
		{
			Insert (idx, anim, (Element) newElement);
		}

		void InsertVisual (int idx, UITableViewRowAnimation anim, int count)
		{
			var root = Parent as RootElement;
			
			if (root == null || root.TableView == null)
				return;
			
			int sidx = root.IndexOf (this);
			var paths = new NSIndexPath [count];
			for (int i = 0; i < count; i++)
				paths [i] = NSIndexPath.FromRowSection (idx+i, sidx);
			
			root.TableView.InsertRows (paths, anim);
		}
		
		public void Insert (int index, params Element [] newElements)
		{
			Insert (index, UITableViewRowAnimation.None, newElements);
		}
		
		public void Remove (Element e)
		{
			if (e == null)
				return;
			for (int i = Elements.Count; i > 0;){
				i--;
				if (Elements [i] == e){
					RemoveRange (i, 1);
					return;
				}
			}
		}
		
		public void Remove (int idx)
		{
			RemoveRange (idx, 1);
		}
		
		/// <summary>
		/// Removes a range of elements from the Section
		/// </summary>
		/// <param name="start">
		/// Starting position
		/// </param>
		/// <param name="count">
		/// Number of elements to remove from the section
		/// </param>
		public void RemoveRange (int start, int count)
		{
			RemoveRange (start, count, UITableViewRowAnimation.Fade);
		}

		/// <summary>
		/// Remove a range of elements from the section with the given animation
		/// </summary>
		/// <param name="start">
		/// Starting position
		/// </param>
		/// <param name="count">
		/// Number of elements to remove form the section
		/// </param>
		/// <param name="anim">
		/// The animation to use while removing the elements
		/// </param>
		public void RemoveRange (int start, int count, UITableViewRowAnimation anim)
		{
			if (start < 0 || start >= Elements.Count)
				return;
			if (count == 0)
				return;
			
			var root = Parent as RootElement;
			
			if (start+count > Elements.Count)
				count = Elements.Count-start;
			
			Elements.RemoveRange (start, count);
			
			if (root == null || root.TableView == null)
				return;
			
			int sidx = root.IndexOf (this);
			var paths = new NSIndexPath [count];
			for (int i = 0; i < count; i++)
				paths [i] = NSIndexPath.FromRowSection (start+i, sidx);
			root.TableView.DeleteRows (paths, anim);
		}
		
		/// <summary>
		/// Enumerator to get all the elements in the Section.
		/// </summary>
		/// <returns>
		/// A <see cref="IEnumerator"/>
		/// </returns>
		public IEnumerator GetEnumerator ()
		{
			foreach (var e in Elements)
				yield return e;
		}

		public int Count {
			get {
				return Elements.Count;
			}
		}

		public Element this [int idx] {
			get {
				return Elements [idx];
			}
		}

		public void Clear ()
		{
			if (Elements != null){
				foreach (var e in Elements)
					e.Dispose ();
			}

            Elements.Clear();

			var root = Parent as RootElement;
			if (root != null && root.TableView != null)
				root.TableView.ReloadData ();
		}

        public void Reset(IEnumerable<Element> elements)
		{
            if (Elements != null)
            {
                foreach (var e in Elements)
                    e.Dispose();
            }

            Elements.Clear();

            Insert(0, UITableViewRowAnimation.None, elements);
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing){
				Parent = null;
				Clear ();
			}
			base.Dispose (disposing);
		}

		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = new UITableViewCell (UITableViewCellStyle.Default, "");
			cell.TextLabel.Text = "Section was used for Element";
			
			return cell;
		}
	}
	
	/// <summary>
	/// Used by root elements to fetch information when they need to
	/// render a summary (Checkbox count or selected radio group).
	/// </summary>
	public class Group {
		public string Key;
		public Group (string key)
		{
			Key = key;
		}
	}
	/// <summary>
	/// Captures the information about mutually exclusive elements in a RootElement
	/// </summary>
	public class RadioGroup : Group {
		int selected;
		public virtual int Selected {
			get { return selected; }
			set { selected = value; }
		}
		
		public RadioGroup (string key, int selected) : base (key)
		{
			this.selected = selected;
		}
		
		public RadioGroup (int selected) : base (null)
		{
			this.selected = selected;
		}
	}
	
	/// <summary>
	///    RootElements are responsible for showing a full configuration page.
	/// </summary>
	/// <remarks>
	///    At least one RootElement is required to start the MonoTouch.Dialogs
	///    process.   
	/// 
	///    RootElements can also be used inside Sections to trigger
	///    loading a new nested configuration page.   When used in this mode
	///    the caption provided is used while rendered inside a section and
	///    is also used as the Title for the subpage.
	/// 
	///    If a RootElement is initialized with a section/element value then
	///    this value is used to locate a child Element that will provide
	///    a summary of the configuration which is rendered on the right-side
	///    of the display.
	/// 
	///    RootElements are also used to coordinate radio elements.  The
	///    RadioElement members can span multiple Sections (for example to
	///    implement something similar to the ring tone selector and separate
	///    custom ring tones from system ringtones).
	/// 
	///    Sections are added by calling the Add method which supports the
	///    C# 4.0 syntax to initialize a RootElement in one pass.
	/// </remarks>
	public class RootElement : Element, IEnumerable, IEnumerable<Section> {
		static NSString rkey1 = new NSString ("RootElement1");
		static NSString rkey2 = new NSString ("RootElement2");
		int summarySection, summaryElement;
		internal Group group;
		public bool UnevenRows;
		public Func<RootElement, UIViewController> createOnSelected;
		public UITableView TableView;
		
		// This is used to indicate that we need the DVC to dispatch calls to
		// WillDisplayCell so we can prepare the color of the cell before 
		// display
		public bool NeedColorUpdate;
		
		/// <summary>
		///  Initializes a RootSection with a caption
		/// </summary>
		/// <param name="caption">
		///  The caption to render.
		/// </param>
		public RootElement (string caption) : base (caption)
		{
			summarySection = -1;
			Sections = new List<Section> ();
		}

		/// <summary>
		/// Initializes a RootSection with a caption and a callback that will
		/// create the nested UIViewController that is activated when the user
		/// taps on the element.
		/// </summary>
		/// <param name="caption">
		///  The caption to render.
		/// </param>
		public RootElement (string caption, Func<RootElement, UIViewController> createOnSelected) : base (caption)
		{
			summarySection = -1;
			this.createOnSelected = createOnSelected;
			Sections = new List<Section> ();
		}
		
		/// <summary>
		///   Initializes a RootElement with a caption with a summary fetched from the specified section and leement
		/// </summary>
		/// <param name="caption">
		/// The caption to render cref="System.String"/>
		/// </param>
		/// <param name="section">
		/// The section that contains the element with the summary.
		/// </param>
		/// <param name="element">
		/// The element index inside the section that contains the summary for this RootSection.
		/// </param>
		public 	RootElement (string caption, int section, int element) : base (caption)
		{
			summarySection = section;
			summaryElement = element;
		}
		
		/// <summary>
		/// Initializes a RootElement that renders the summary based on the radio settings of the contained elements. 
		/// </summary>
		/// <param name="caption">
		/// The caption to ender
		/// </param>
		/// <param name="group">
		/// The group that contains the checkbox or radio information.  This is used to display
		/// the summary information when a RootElement is rendered inside a section.
		/// </param>
		public RootElement (string caption, Group group) : base (caption)
		{
			this.group = group;
		}
		
		internal List<Section> Sections = new List<Section> ();

		internal NSIndexPath PathForRadio (int idx)
		{
			RadioGroup radio = group as RadioGroup;
			if (radio == null)
				return null;
			
			uint current = 0, section = 0;
			foreach (Section s in Sections){
				uint row = 0;
				
				foreach (Element e in s.Elements){
					if (!(e is RadioElement))
						continue;
					
					if (current == idx){
						return NSIndexPath.Create(section, row); 
					}
					row++;
					current++;
				}
				section++;
			}
			return null;
		}
		
		public int Count { 
			get {
				return Sections.Count;
			}
		}

		public Section this [int idx] {
			get {
				return Sections [idx];
			}
		}
		
		internal int IndexOf (Section target)
		{
			int idx = 0;
			foreach (Section s in Sections){
				if (s == target)
					return idx;
				idx++;
			}
			return -1;
		}
			
		public void Prepare ()
		{
			int current = 0;
			foreach (Section s in Sections){				
				foreach (Element e in s.Elements){
					var re = e as RadioElement;
					if (re != null)
						re.RadioIdx = current++;
					if (UnevenRows == false && e is IElementSizing)
						UnevenRows = true;
					if (NeedColorUpdate == false && e is IColorizeBackground)
						NeedColorUpdate = true;
				}
			}
		}
		
		/// <summary>
		/// Adds a new section to this RootElement
		/// </summary>
		/// <param name="section">
		/// The section to add, if the root is visible, the section is inserted with no animation
		/// </param>
		public void Add (Section section)
		{
			if (section == null)
				return;
			
			Sections.Add (section);
			section.Parent = this;
			if (TableView == null)
				return;
			
			TableView.InsertSections (MakeIndexSet (Sections.Count-1, 1), UITableViewRowAnimation.None);
		}

		//
		// This makes things LINQ friendly;  You can now create RootElements
		// with an embedded LINQ expression, like this:
		// new RootElement ("Title") {
		//     from x in names
		//         select new Section (x) { new StringElement ("Sample") }
		//
		public void Add (IEnumerable<Section> sections)
		{
			foreach (var s in sections)
				Add (s);
		}
		
		NSIndexSet MakeIndexSet (int start, int count)
		{
			NSRange range;
			range.Location = start;
			range.Length = count;
			return NSIndexSet.FromNSRange (range);
		}
		
		/// <summary>
		/// Inserts a new section into the RootElement
		/// </summary>
		/// <param name="idx">
		/// The index where the section is added <see cref="System.Int32"/>
		/// </param>
		/// <param name="anim">
		/// The <see cref="UITableViewRowAnimation"/> type.
		/// </param>
		/// <param name="newSections">
		/// A <see cref="Section[]"/> list of sections to insert
		/// </param>
		/// <remarks>
		///    This inserts the specified list of sections (a params argument) into the
		///    root using the specified animation.
		/// </remarks>
		public void Insert (int idx, UITableViewRowAnimation anim, params Section [] newSections)
		{
			if (idx < 0 || idx > Sections.Count)
				return;
			if (newSections == null)
				return;
			
			if (TableView != null)
				TableView.BeginUpdates ();
			
			int pos = idx;
			foreach (var s in newSections){
				s.Parent = this;
				Sections.Insert (pos++, s);
			}
			
			if (TableView == null)
				return;
			
			TableView.InsertSections (MakeIndexSet (idx, newSections.Length), anim);
			TableView.EndUpdates ();
		}
		
		/// <summary>
		/// Inserts a new section into the RootElement
		/// </summary>
		/// <param name="idx">
		/// The index where the section is added <see cref="System.Int32"/>
		/// </param>
		/// <param name="newSections">
		/// A <see cref="Section[]"/> list of sections to insert
		/// </param>
		/// <remarks>
		///    This inserts the specified list of sections (a params argument) into the
		///    root using the Fade animation.
		/// </remarks>
		public void Insert (int idx, Section section)
		{
			Insert (idx, UITableViewRowAnimation.None, section);
		}
		
		/// <summary>
		/// Removes a section at a specified location
		/// </summary>
		public void RemoveAt (int idx)
		{
			RemoveAt (idx, UITableViewRowAnimation.Fade);
		}

		/// <summary>
		/// Removes a section at a specified location using the specified animation
		/// </summary>
		/// <param name="idx">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="anim">
		/// A <see cref="UITableViewRowAnimation"/>
		/// </param>
		public void RemoveAt (int idx, UITableViewRowAnimation anim)
		{
			if (idx < 0 || idx >= Sections.Count)
				return;
			
			Sections.RemoveAt (idx);
			
			if (TableView == null)
				return;
			
			TableView.DeleteSections (NSIndexSet.FromIndex (idx), anim);
		}
			
		public void Remove (Section s)
		{
			if (s == null)
				return;
			int idx = Sections.IndexOf (s);
			if (idx == -1)
				return;
			RemoveAt (idx, UITableViewRowAnimation.Fade);
		}
		
		public void Remove (Section s, UITableViewRowAnimation anim)
		{
			if (s == null)
				return;
			int idx = Sections.IndexOf (s);
			if (idx == -1)
				return;
			RemoveAt (idx, anim);
		}

		public void Clear ()
		{
			foreach (var s in Sections)
				s.Dispose ();
			Sections = new List<Section> ();
			if (TableView != null)
				TableView.ReloadData ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing){
				if (Sections == null)
					return;
				
				TableView = null;
				Clear ();
				Sections = null;
			}
		}
		
		/// <summary>
		/// Enumerator that returns all the sections in the RootElement.
		/// </summary>
		/// <returns>
		/// A <see cref="IEnumerator"/>
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			foreach (var s in Sections)
				yield return s;
		}
		
		IEnumerator<Section> IEnumerable<Section>.GetEnumerator ()
		{
			foreach (var s in Sections)
				yield return s;
		}

		/// <summary>
		/// The currently selected Radio item in the whole Root.
		/// </summary>
		public int RadioSelected {
			get {
				var radio = group as RadioGroup;
				if (radio != null)
					return radio.Selected;
				return -1;
			}
			set {
				var radio = group as RadioGroup;
				if (radio != null)
					radio.Selected = value;
			}
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			NSString key = summarySection == -1 ? rkey1 : rkey2;
			var cell = tv.DequeueReusableCell (key);
			if (cell == null){
				var style = summarySection == -1 ? UITableViewCellStyle.Default : UITableViewCellStyle.Value1;
				
				cell = new UITableViewCell (style, key);
				cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
			} 
		
			cell.TextLabel.Text = Caption;
			var radio = group as RadioGroup;
			if (radio != null){
				int selected = radio.Selected;
				int current = 0;
				
				foreach (var s in Sections){
					foreach (var e in s.Elements){
						if (!(e is RadioElement))
							continue;
						
						if (current == selected){
							cell.DetailTextLabel.Text = e.Summary ();
							goto le;
						}
						current++;
					}
				}
			} else if (group != null){
				int count = 0;
				
				foreach (var s in Sections){
					foreach (var e in s.Elements){
						var ce = e as CheckboxElement;
						if (ce != null){
							if (ce.Value)
								count++;
							continue;
						}
						var be = e as BoolElement;
						if (be != null){
							if (be.Value)
								count++;
							continue;
						}
					}
				}
				cell.DetailTextLabel.Text = count.ToString ();
			} else if (summarySection != -1 && summarySection < Sections.Count){
					var s = Sections [summarySection];
					if (summaryElement < s.Elements.Count && cell.DetailTextLabel != null)
						cell.DetailTextLabel.Text = s.Elements [summaryElement].Summary ();
			} 
		le:
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			
			return cell;
		}
		
		/// <summary>
		///    This method does nothing by default, but gives a chance to subclasses to
		///    customize the UIViewController before it is presented
		/// </summary>
		protected virtual void PrepareDialogViewController (UIViewController dvc)
		{
		}
		
		/// <summary>
		/// Creates the UIViewController that will be pushed by this RootElement
		/// </summary>
		protected virtual UIViewController MakeViewController ()
		{
			if (createOnSelected != null)
				return createOnSelected (this);
			
			return new DialogViewController (this, true) {
				Autorotate = true
			};
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			tableView.DeselectRow (path, false);
			var newDvc = MakeViewController ();
			PrepareDialogViewController (newDvc);
			dvc.ActivateController (newDvc);
		}
		
		public void Reload (Section section, UITableViewRowAnimation animation)
		{
			if (section == null)
				throw new ArgumentNullException ("section");
			if (section.Parent == null || section.Parent != this)
				throw new ArgumentException ("Section is not attached to this root");
			
			int idx = 0;
			foreach (var sect in Sections){
				if (sect == section){
					TableView.ReloadSections (new NSIndexSet ((uint) idx), animation);
					return;
				}
				idx++;
			}
		}
		
		public void Reload (Element element, UITableViewRowAnimation animation)
		{
			if (element == null)
				throw new ArgumentNullException ("element");
			var section = element.Parent as Section;
			if (section == null)
				throw new ArgumentException ("Element is not attached to this root");
			var root = section.Parent as RootElement;
			if (root == null)
				throw new ArgumentException ("Element is not attached to this root");
			var path = element.IndexPath;
			if (path == null)
				return;
			TableView.ReloadRows (new NSIndexPath [] { path }, animation);
		}
		
	}
}
