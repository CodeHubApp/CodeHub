using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UIKit;
using Foundation;
using CoreGraphics;

namespace MonoTouch.Dialog
{
	/// <summary>
	/// An element that can be used to enter text.
	/// </summary>
	/// <remarks>
	/// This element can be used to enter text both regular and password protected entries. 
	///     
	/// The Text fields in a given section are aligned with each other.
	/// </remarks>
	public class MultiLineEntryElement : Element, IElementSizing  {
		public int Rows;
		public string Value
		{
			get { return _val; }
			set
			{
				_val = value;
				if (ValueChanged != null)
					ValueChanged(this, EventArgs.Empty);
			}				
		}
		private string _val;
		public event EventHandler ValueChanged;
			
		private static NSString ekey = new NSString ("EntryElement");
		private UITextView entry;
		private static UIFont font = UIFont.SystemFontOfSize (17);
		
		/// <summary>
		/// Constructs an EntryElement with the given caption, placeholder and initial value.
		/// </summary>
		/// <param name="caption">
		/// The caption to use
		/// </param>
		/// <param name="placeholder">
		/// Placeholder to display.
		/// </param>
		/// <param name="value">
		/// Initial value.
		/// </param>
		public MultiLineEntryElement (string caption, string value)
			: base (caption)
		{
			Value = value;
		}
		
		public bool IsResponder()
		{
			return entry == null ? false : entry.IsFirstResponder;
		}
		/// <summary>
		/// Constructs  an EntryElement for password entry with the given caption, placeholder and initial value.
		/// </summary>
		/// <param name="caption">
		/// The caption to use
		/// </param>
		/// <param name="placeholder">
		/// Placeholder to display.
		/// </param>
		/// <param name="value">
		/// Initial value.
		/// </param>
		/// <param name="isPassword">
		/// True if this should be used to enter a password.
		/// </param>

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

			CGSize max = new CGSize (-1, -1);
			foreach (var e in s.Elements){
				var ee = e as MultiLineEntryElement;
				if (ee == null)
					continue;

                var size = new NSString(ee.Caption).StringSize (font);
				if (size.Width > max.Width)
					max = size;				
			}
			s.EntryAlignment = new CGSize (25 + Math.Min (max.Width, 160), max.Height);
			return s.EntryAlignment;
		}

		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (ekey);
			if (cell == null){
				cell = new MultiLineTableCell (UITableViewCellStyle.Subtitle, ekey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			} else 
				RemoveTag (cell, 1);


			if (entry == null)
			{
				entry = new UITextView(new CGRect(0, 28, cell.ContentView.Bounds.Width, 16 + (20 * (Rows + 1))));
				entry.Font = UIFont.SystemFontOfSize(16);
				entry.Text = Value ?? "";
				entry.BackgroundColor = UIColor.Clear;
				entry.AutoresizingMask = UIViewAutoresizing.FlexibleWidth |
					UIViewAutoresizing.FlexibleLeftMargin;
				
				entry.Ended += delegate {
					Value = entry.Text;
					
					entry.ResignFirstResponder();
				};
				entry.Changed += delegate(object sender, EventArgs e) {
					if(!string.IsNullOrEmpty(entry.Text))
					{
						int i = entry.Text.IndexOf("\n", entry.Text.Length -1);
						if (i > -1)
						{
							entry.Text = entry.Text.Substring(0,entry.Text.Length -1); 
							entry.ResignFirstResponder();	
						}
					}
					Value = entry.Text;
				};
				////////////////
//				entry.Started += delegate {
//					MultiLineEntryElement focus = null;
//					foreach (var e in (Parent as Section).Elements){
//						if (e == this)
//							focus = this;
//						else if (focus != null && e is MultiLineEntryElement)
//							focus = e as MultiLineEntryElement;
//					}
//					if (focus != this)
//						focus.entry.BecomeFirstResponder ();
//					else 
//						focus.entry.ResignFirstResponder ();
//
//				};
				
//				entry.Started += delegate {
//					MultiLineEntryElement self = null;
//					var returnType = UIReturnKeyType.Default;
//					
//					foreach (var e in (Parent as Section).Elements){
//						if (e == this)
//							self = this;
//						else if (self != null && e is MultiLineEntryElement)
//							returnType = UIReturnKeyType.Next;
//					}
//				};
				
				entry.ReturnKeyType = UIReturnKeyType.Done;
			}
			///////////
			cell.TextLabel.Text = Caption;
			
			cell.ContentView.AddSubview (entry);
			return cell;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing && entry != null)
			{
				entry.Dispose ();
				entry = null;
			}
		}

		public nfloat GetHeight (UITableView tableView, NSIndexPath indexPath)
		{	
			return 44 + (20 * (Rows + 1));
		}
	}
}

