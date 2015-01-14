using System;
using System.Drawing;
using MonoTouch.UIKit;

namespace CodeHub.iOS.ViewComponents
{
    public class PickerAlertView : UIView
    {
        private readonly string[] _values;
        private readonly int _currentSelected;
        private readonly Action<int> _selected;
        private readonly UIPickerView _pickerView;
        private readonly UIToolbar _toolbar;
        private readonly UIView _innerView;

        public PickerAlertView(string[] values, int currentSelected, Action<int> selected)
            : base(new RectangleF(0, 0, 320f, 480f))
        {
            AutosizesSubviews = true;
            this.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

            _values = values;
            _currentSelected = currentSelected;
            _selected = selected;

            _pickerView = new UIPickerView();
            _pickerView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            _pickerView.ShowSelectionIndicator = true;
            _pickerView.Model = new PickerModel(values);
            _pickerView.BackgroundColor = UIColor.FromRGB(244, 244, 244);
            _pickerView.Select(currentSelected, 0, false);

            _toolbar = new UIToolbar();
            _toolbar.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin;

            _toolbar.Items = new UIBarButtonItem[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem("Done", UIBarButtonItemStyle.Done, (s, e) => {
                    _selected(_pickerView.SelectedRowInComponent(0));
                    Dismiss();
                })
            };

            _innerView = new UIView(new RectangleF(0, Frame.Height, Frame.Width, 44f + _pickerView.Frame.Height));
            _innerView.AutosizesSubviews = true;
            _innerView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin;

            _toolbar.Frame = new RectangleF(0, 0, Frame.Width, 44f);
            _innerView.Add(_toolbar);

            _pickerView.Frame = new RectangleF(0, 44f, Frame.Width, _pickerView.Frame.Height);
            _innerView.Add(_pickerView);

            Add(_innerView);

        }

        public override void TouchesBegan(MonoTouch.Foundation.NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            var touch = touches.AnyObject as UITouch;
            if (touch != null)
            {
                if (touch.LocationInView(this).Y < _innerView.Frame.Y)
                    Dismiss();
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            Frame = Superview.Bounds;
        }


        private void Dismiss()
        {
            UIView.Animate(0.25, 0, UIViewAnimationOptions.CurveEaseIn, () =>
                _innerView.Frame = new RectangleF(0, Frame.Height, _innerView.Frame.Width, _innerView.Frame.Height), RemoveFromSuperview);
        }

        private void Present()
        {
            UIView.Animate(0.25, 0, UIViewAnimationOptions.CurveEaseIn, () =>
                _innerView.Frame = new RectangleF(0, Frame.Height - _innerView.Frame.Height, _innerView.Frame.Width, _innerView.Frame.Height), null);
        }


        public void Show()
        {
            var window = UIApplication.SharedApplication.Delegate.Window.RootViewController.View;
            Frame = window.Bounds;
            window.AddSubview(this);
            Present();
        }

        private class PickerModel : UIPickerViewModel
        {
            private readonly string[] _values;
            public PickerModel(string[] values)
            {
                _values = values;
            }

            public override int GetComponentCount(UIPickerView picker)
            {
                return 1;
            }

            public override int GetRowsInComponent(UIPickerView picker, int component)
            {
                return _values.Length;
            }

            public override string GetTitle(UIPickerView picker, int row, int component)
            {

                return _values[row];
            }
        }
    }
}

