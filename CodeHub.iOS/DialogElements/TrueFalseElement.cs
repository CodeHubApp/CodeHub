using System;
using MonoTouch.UIKit;

namespace CodeHub.iOS.DialogElements
{
    public class BooleanElement : Element 
    {
        private UISwitch _switch;
        private bool _value;

        public event EventHandler ValueChanged;

        public bool Value 
        {
            get 
            {
                return _value;
            }
            set 
            {
                bool emit = _value != value;
                _value = value;
                if (_switch != null)
                    _switch.On = value;
                if (emit && ValueChanged != null)
                    ValueChanged (this, EventArgs.Empty);
            }
        }

        public BooleanElement (string caption, bool value, Action<BooleanElement> changeAction = null) 
        {  
            Caption = caption;
            _value = value;

            if (changeAction != null)
                this.ValueChanged += (sender, e) => changeAction(this);
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            if (_switch == null)
            {
                _switch = new UISwitch
                {
                    BackgroundColor = UIColor.Clear,
                    Tag = 1,
                    On = Value
                };
                _switch.AddTarget(delegate
                {
                    Value = _switch.On;
                }, UIControlEvent.ValueChanged);
            }
            else
            {
                _switch.On = Value;
            }

            var cell = tv.DequeueReusableCell ("boolean_element");
            if (cell == null){
                cell = new UITableViewCell (UITableViewCellStyle.Default, "boolean_element");
                cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            }

            cell.BackgroundColor = StyledStringElement.BgColor;
            cell.TextLabel.Font = StyledStringElement.DefaultTitleFont.WithSize(StyledStringElement.DefaultTitleFont.PointSize);
            cell.TextLabel.TextColor = StyledStringElement.DefaultTitleColor;
            cell.TextLabel.Text = Caption;
            cell.AccessoryView = _switch;
            return cell;
        }
    }
}

