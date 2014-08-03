using System;
using System.Linq;
using MonoTouch.UIKit;
using Xamarin.Utilities.ViewControllers;
using Xamarin.Utilities.DialogElements;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.iOS.ViewControllers
{
    public abstract class FilterViewController<TViewModel> : ViewModelDialogViewController<TViewModel> where TViewModel : class, IBaseViewModel
    {
        protected FilterViewController()
        {
            Title = "Filter & Sort";
            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(Images.Cancel, UIBarButtonItemStyle.Plain, (s, e) => 
                DismissViewController(true, null));
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

        public class EnumChoiceElement<T> : StyledStringElement where T : struct, IConvertible
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
                var ctrl = new DialogViewController {Title = title};
                var sec = new Section();
                foreach (var x in Enum.GetValues(typeof(T)).Cast<System.Enum>())
                {
                    sec.Add(new StyledStringElement(x.Description(), () => { 
                        element.Value = (T)Enum.ToObject(typeof(T), x); 
                        NavigationController.PopViewControllerAnimated(true);
                    }) { 
                        Accessory = object.Equals(x, element.Value) ? 
                            MonoTouch.UIKit.UITableViewCellAccessory.Checkmark : MonoTouch.UIKit.UITableViewCellAccessory.None 
                    });
                }
                ctrl.Root.Reset(sec);
                NavigationController.PushViewController(ctrl, true);
            };
            
            return element;
        }

        public class MultipleChoiceElement<T> : StyledStringElement
        {
            public T Obj;
            public MultipleChoiceElement(string title, T obj)
                : base(title, CreateCaptionForMultipleChoice(obj), UITableViewCellStyle.Value1)
            {
                Obj = obj;
                Accessory = UITableViewCellAccessory.DisclosureIndicator;
            }
        }

//        protected MultipleChoiceElement<T> CreateMultipleChoiceElement<T>(string title, T o)
//        {
//            var element = new MultipleChoiceElement<T>(title, o);
//            element.Tapped += () =>
//            {
//                var en = new MultipleChoiceViewController(element.Caption, o);
//                en.ViewDisappearing += (sender, e) => {
//                    element.Value = CreateCaptionForMultipleChoice(o);
//                };
//                NavigationController.PushViewController(en, true);
//            };
//
//            return element;
//        }

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
                return trueCounter == fields.Length ? "Any" : str.Substring(0, str.Length - 2);
            return "None";
        }
    }
}

