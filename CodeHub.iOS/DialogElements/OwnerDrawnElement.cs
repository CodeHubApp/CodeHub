using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;

namespace CodeHub.iOS.DialogElements
{
	public abstract class OwnerDrawnElement : Element, IElementSizing
	{	
        private string _key;

		public UITableViewCellStyle Style
		{
			get;set;	
		}
		
        protected OwnerDrawnElement (UITableViewCellStyle style, string key)
		{
			this.Style = style;
            _key = key;
		}
		
		public float GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			return Height(tableView.Bounds);
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
            var cell = tv.DequeueReusableCell(_key) as OwnerDrawnCell;
			if (cell == null)
			{
                cell = new OwnerDrawnCell(this, this.Style, _key);
                cell.AutosizesSubviews = true;
                cell.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
                CellCreated(cell, cell.view);
			}
			else
			{
				cell.Element = this;
			}
			
			cell.Update();
			return cell;
		}	
		
        protected virtual void CellCreated(UITableViewCell cell, UIView view)
        {
        }

		public abstract void Draw(RectangleF bounds, CGContext context, UIView view);
		
		public abstract float Height(RectangleF bounds);
		
		class OwnerDrawnCell : UITableViewCell
		{
			public OwnerDrawnCellView view;
			
			public OwnerDrawnCell(OwnerDrawnElement element, UITableViewCellStyle style, string cellReuseIdentifier) : base(style, cellReuseIdentifier)
			{
				Element = element;
			}
			
			public OwnerDrawnElement Element
			{
				get {
					return view.Element;
				}
				set {
					if (view == null)
					{
						view = new OwnerDrawnCellView (value);
						ContentView.Add (view);
					}
					else
					{
						view.Element = value;
					}
				}
			}
				
			

			public void Update()
			{
				SetNeedsDisplay();
				view.SetNeedsDisplay();
			}		
	
			public override void LayoutSubviews()
			{
				base.LayoutSubviews();
				
				view.Frame = ContentView.Bounds;
			}
		}
		
		class OwnerDrawnCellView : UIView
		{				
			OwnerDrawnElement element;
			
			public OwnerDrawnCellView(OwnerDrawnElement element)
			{
				this.element = element;
                this.AutosizesSubviews = true;
                this.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
                this.BackgroundColor = UIColor.Clear;
			}
			
			
			public OwnerDrawnElement Element
			{
				get { return element; }
				set {
					element = value; 
				}
			}
			
			public void Update()
			{
				SetNeedsDisplay();
			
			}
			
			public override void Draw (RectangleF rect)
			{
				CGContext context = UIGraphics.GetCurrentContext();
				element.Draw(rect, context, this);
			}
		}
	}
}

