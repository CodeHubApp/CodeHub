using System;
using UIKit;
using CodeHub.iOS.DialogElements;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace CodeHub.iOS.DialogElements
{
    public class BooleanElement : Element 
    {
        private readonly Subject<bool> _changedSubject = new Subject<bool>();
        private bool _value;

        public IObservable<bool> Changed
        {
            get { return _changedSubject.AsObservable(); }
        }

        private string _caption;
        public string Caption
        {
            get { return _caption; }
            set
            {
                if (_caption == value)
                    return;

                _caption = value;
                var cell = GetActiveCell();
                if (cell != null && cell.TextLabel != null)
                    cell.TextLabel.Text = value ?? string.Empty;
            }
        }

        public bool Value 
        {
            get 
            {
                return _value;
            }
            set 
            {
                if (_value == value)
                    return;
                
                _value = value;
                _changedSubject.OnNext(value);
                var cell = GetActiveCell() as BooleanCellView;
                if (cell != null)
                    cell.Switch.On = value;
            }
        }

        public BooleanElement (string caption, bool value = false) 
        {  
            Caption = caption;
            _value = value;
        }

        public override UITableViewCell GetCell (UITableView tv)
        {
            var cell = tv.DequeueReusableCell("boolean_element") as BooleanCellView ?? new BooleanCellView();
            cell.Switch.On = Value;
            cell.BackgroundColor = StringElement.BgColor;
            cell.TextLabel.Font = UIFont.PreferredBody;
            cell.TextLabel.TextColor = StringElement.DefaultTitleColor;
            cell.TextLabel.Text = Caption;

            var weakThis = new WeakReference<BooleanElement>(this);
            cell.Switch.ValueChanged += UpdateValueChanged(weakThis);

            return cell;
        }

        private static EventHandler UpdateValueChanged(WeakReference<BooleanElement> weakThis)
        {
            return new EventHandler((s, _) => {
                BooleanElement parent;
                if (weakThis.TryGetTarget(out parent))
                    parent.Value = ((UISwitch)s).On;
            });
        }

        private class BooleanCellView : UITableViewCell
        {
            public UISwitch Switch { get; }

            public BooleanCellView()
                : base(UITableViewCellStyle.Default, "boolean_element")
            {
                Switch = new UISwitch();
                Switch.BackgroundColor = UIColor.Clear;
                SelectionStyle = UITableViewCellSelectionStyle.None;
            }

            public override void WillMoveToSuperview(UIView newsuper)
            {
                base.WillMoveToSuperview(newsuper);
                AccessoryView = newsuper == null ? null : Switch;
            }
        }
    }
}

