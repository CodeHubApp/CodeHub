using System;
using CoreGraphics;
using UIKit;
using Foundation;

namespace CodeHub.iOS.DialogElements
{
    public abstract class OwnerDrawnElement : Element, IElementSizing
    {        
        public string CellReuseIdentifier
        {
            get;set;    
        }
        
        public UITableViewCellStyle Style
        {
            get;set;    
        }
        
        public OwnerDrawnElement (UITableViewCellStyle style, string cellIdentifier)
        {
            this.CellReuseIdentifier = cellIdentifier;
            this.Style = style;
        }
        
        public nfloat GetHeight (UITableView tableView, NSIndexPath indexPath)
        {
            return Height(tableView.Bounds);
        }
        
        public override UITableViewCell GetCell (UITableView tv)
        {
            OwnerDrawnCell cell = tv.DequeueReusableCell(this.CellReuseIdentifier) as OwnerDrawnCell;
            
            if (cell == null)
            {
                cell = new OwnerDrawnCell(this, this.Style, this.CellReuseIdentifier);
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

        public abstract void Draw(CGRect bounds, CGContext context, UIView view);
        
        public abstract nfloat Height(CGRect bounds);
        
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
            
            public override void Draw (CGRect rect)
            {
                CGContext context = UIGraphics.GetCurrentContext();
                element.Draw(rect, context, this);
            }
        }
    }
}

