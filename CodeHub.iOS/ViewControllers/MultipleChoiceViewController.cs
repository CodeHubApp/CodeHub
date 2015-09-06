using CodeFramework.ViewControllers;
using MonoTouch.Dialog;
using UIKit;

namespace CodeFramework.iOS.ViewControllers
{
    public class MultipleChoiceViewController : BaseDialogViewController
    {
        private readonly object _obj;
        
        protected void OnValueSelected(System.Reflection.PropertyInfo field)
        {
            var r = Root[0].Elements.Find(x => x.Caption.Equals(field.Name));
            if (r == null)
                return;
            var e = (StyledStringElement)r;
            var value = (bool)field.GetValue(_obj);
            field.SetValue(_obj, !value);
            e.Accessory = !value ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
            Root.Reload(e, UITableViewRowAnimation.None);
        }
        
        public MultipleChoiceViewController(string title, object obj)
            : base (true)
        {
            _obj = obj;
            Title = title;
            Style = UITableViewStyle.Grouped;

            var sec = new Section();
            var fields = obj.GetType().GetProperties();
            foreach (var s in fields)
            {
                var copy = s;
                sec.Add(new StyledStringElement(s.Name, () => OnValueSelected(copy)) { 
                    Accessory = (bool)s.GetValue(_obj) ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None 
                });
            }
            Root.Add(sec);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Root.Caption = Title;
        }
    }
}

