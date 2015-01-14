using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace CodeHub.iOS.DialogElements
{
    public abstract class CustomElement : Element, IElementSizing, IColorizeBackground
    {       
        public string CellReuseIdentifier
        {
            get;set;    
        }

        public UITableViewCellStyle Style
        {
            get;set;    
        }

        public UIColor BackgroundColor { get; set; }

        protected CustomElement (UITableViewCellStyle style, string cellIdentifier)
        {
            CellReuseIdentifier = cellIdentifier;
            Style = style;
        }

        public float GetHeight (UITableView tableView, NSIndexPath indexPath)
        {
            try
            {
                if (tableView.Style == UITableViewStyle.Grouped)
                {
					//float margin = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone ? 20f : 110f;
					SizeF size = new SizeF (tableView.Bounds.Width /* - margin */, float.MaxValue);
                    return Height(tableView, new RectangleF(tableView.Bounds.Location, size));
                }
                return Height(tableView, tableView.Bounds);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Attempted to get cell height resulted in exception: " + e.Message);
            }
            return 0f;
        }

        
        public event NSAction Tapped;

        public bool IsTappedAssigned { get { return Tapped != null; } }

        protected virtual void OnCreateCell(UITableViewCell cell)
        {
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            var cell = tv.DequeueReusableCell(CellReuseIdentifier) as OwnerDrawnCell;

            if (cell == null)
            {
                cell = new OwnerDrawnCell(this, Style, CellReuseIdentifier);
                OnCreateCell(cell);
            }
            else
            {
                cell.Element = this;
            }

            if (Tapped != null) {
                cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
            } 
            else 
            {
                cell.Accessory = UITableViewCellAccessory.None;
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            }

            cell.BackgroundColor = BackgroundColor;
            cell.Update();
            return cell;
        }   

        public override void Selected(UITableView tableView, NSIndexPath path)
        {
            base.Selected(tableView, path);
            if (Tapped != null)
                Tapped();
            tableView.DeselectRow (path, true);
        }

        public abstract void Draw(RectangleF bounds, CGContext context, UIView view);

        public abstract float Height(UITableView tableView, RectangleF bounds);

        class OwnerDrawnCell : UITableViewCell
        {
            OwnerDrawnCellView _view;

            public OwnerDrawnCell(CustomElement element, UITableViewCellStyle style, string cellReuseIdentifier) : base(style, cellReuseIdentifier)
            {
                Element = element;
            }

            public CustomElement Element
            {
                get {
                    return _view.Element;
                }
                set {
                    if (_view == null)
                    {
                        _view = new OwnerDrawnCellView (value);
                        ContentView.Add (_view);
                    }
                    else
                    {
                        _view.Element = value;
                    }
                }
            }



            public void Update()
            {
                SetNeedsDisplay();
                _view.SetNeedsDisplay();
            }       

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                _view.Frame = ContentView.Bounds;
                _view.SetNeedsDisplay();
            }
        }

        class OwnerDrawnCellView : UIView
        {               
            CustomElement _element;

            public OwnerDrawnCellView(CustomElement element)
            {
                _element = element;
                BackgroundColor = UIColor.Clear;
            }


            public CustomElement Element
            {
                get { return _element; }
                set {
                    _element = value; 
                }
            }

            public void Update()
            {
                SetNeedsDisplay();

            }

            public override void Draw (RectangleF rect)
            {
                base.Draw(rect);
                try
                {
                    CGContext context = UIGraphics.GetCurrentContext();
                    _element.Draw(rect, context, this);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Unable to draw: " + e.Message);
                }
            }
        }

        public virtual void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            cell.BackgroundColor = BackgroundColor;
        }

    }
}

