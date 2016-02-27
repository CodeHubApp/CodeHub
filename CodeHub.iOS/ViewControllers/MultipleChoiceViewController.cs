using System;
using CodeHub.iOS.ViewControllers;
using UIKit;
using CodeHub.iOS.DialogElements;
using System.Linq;

namespace CodeHub.iOS.ViewControllers
{
    public class MultipleChoiceViewController : DialogViewController
    {
        private readonly object _obj;
        
        protected void OnValueSelected(System.Reflection.PropertyInfo field)
        {
            var r = Root[0].Elements.OfType<StringElement>().FirstOrDefault(x => x.Caption.Equals(field.Name));
            if (r == null)
                return;
            var value = (bool)field.GetValue(_obj);
            field.SetValue(_obj, !value);
            r.Accessory = !value ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
        }
        
        public MultipleChoiceViewController(string title, object obj)
            : base (UITableViewStyle.Grouped)
        {
            _obj = obj;
            Title = title;

            var sec = new Section();
            var fields = obj.GetType().GetProperties();
            foreach (var s in fields)
            {
                var copy = s;
                var accessory = (bool)s.GetValue(_obj) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
                var e = new StringElement(s.Name) { Accessory = accessory };
                e.Clicked.Subscribe(_ => OnValueSelected(copy));
                sec.Add(e);
            }
            Root.Add(sec);
        }
    }
}

