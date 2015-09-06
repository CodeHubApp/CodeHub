using System;
using System.Linq;
using CodeFramework.iOS.Views;
using CodeFramework.ViewControllers;
using UIKit;

namespace CodeFramework.iOS.ViewControllers
{
    public abstract class FilterViewController : BaseDialogViewController
    {
        protected FilterViewController()
            : base(true)
        {
            Style = UITableViewStyle.Grouped;
            Title = "Filter & Sort".t();
			NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.CancelButton, UIBarButtonItemStyle.Plain, (s, e) => DismissViewController(true, null));
			NavigationItem.RightBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.SaveButton, UIBarButtonItemStyle.Plain, (s, e) => {
                ApplyButtonPressed();
                DismissViewController(true, null); 
            });
        }

        public abstract void ApplyButtonPressed();

        public void CloseViewController()
        {
            DismissViewController(true, null);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            TableView.ReloadData();
        }

        public class EnumChoiceElement<T> : MonoTouch.Dialog.StyledStringElement where T : struct, IConvertible
        {
            private T _value;

            public new T Value
            {
                get { return _value; }
                set
                {
                    _value = value;
                    base.Value = ((Enum)Enum.ToObject(typeof(T), value)).Description();
                }
            }

            public EnumChoiceElement(string title, T defaultVal)
                : base(title, string.Empty, UITableViewCellStyle.Value1)
            {
                Accessory = UITableViewCellAccessory.DisclosureIndicator;
                Value = defaultVal;
            }
        }

        public EnumChoiceElement<T> CreateEnumElement<T>(string title, T value) where T : struct, IConvertible
        {
            var element = new EnumChoiceElement<T>(title, value);

            element.Tapped += () =>
            {
                var ctrl = new BaseDialogViewController(true);
                ctrl.Title = title;
                ctrl.Style = UIKit.UITableViewStyle.Grouped;

                var sec = new MonoTouch.Dialog.Section();
                foreach (var x in System.Enum.GetValues(typeof(T)).Cast<System.Enum>())
                {
                    sec.Add(new MonoTouch.Dialog.StyledStringElement(x.Description(), () => { 
                        element.Value = (T)Enum.ToObject(typeof(T), x); 
                        NavigationController.PopViewController(true);
                    }) { 
                        Accessory = object.Equals(x, element.Value) ? 
                            UIKit.UITableViewCellAccessory.Checkmark : UIKit.UITableViewCellAccessory.None 
                    });
                }
                ctrl.Root = new MonoTouch.Dialog.RootElement(title) { sec };
                NavigationController.PushViewController(ctrl, true);
            };
            
            return element;
        }

        public class MultipleChoiceElement<T> : MonoTouch.Dialog.StyledStringElement
        {
            public T Obj;
            public MultipleChoiceElement(string title, T obj)
                : base(title, CreateCaptionForMultipleChoice(obj), UITableViewCellStyle.Value1)
            {
                Obj = obj;
                Accessory = UITableViewCellAccessory.DisclosureIndicator;
            }
        }

        protected MultipleChoiceElement<T> CreateMultipleChoiceElement<T>(string title, T o)
        {
            var element = new MultipleChoiceElement<T>(title, o);
            element.Tapped += () =>
            {
                var en = new MultipleChoiceViewController(element.Caption, o);
                en.ViewDisappearing += (sender, e) => {
                    element.Value = CreateCaptionForMultipleChoice(o);
                };
                NavigationController.PushViewController(en, true);
            };

            return element;
        }

        private static string CreateCaptionForMultipleChoice<T>(T o)
        {
            var fields = o.GetType().GetFields();
            var sb = new System.Text.StringBuilder();
            int trueCounter = 0;
            foreach (var f in fields)
            {
                if ((bool)f.GetValue(o))
                {
                    sb.Append(f.Name);
                    sb.Append(", ");
                    trueCounter++;
                }
            }
            var str = sb.ToString();
            if (str.EndsWith(", "))
                return trueCounter == fields.Length ? "Any".t() : str.Substring(0, str.Length - 2);
            return "None".t();
        }
    }
}

