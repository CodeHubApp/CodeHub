using System;
using ReactiveUI;

namespace CodeHub.iOS.DialogElements
{
    public static class ElementExtensions
    {
        public static StringElement BindCommand(this StringElement stringElement, Func<IReactiveCommand> cmd)
        {
            stringElement.Tapped = () => cmd().ExecuteIfCan();
            return stringElement;
        }

        public static T BindCaption<T>(this T stringElement, IObservable<string> caption) where T : Element
        {
            caption.Subscribe(x => stringElement.Caption = x);
            return stringElement;
        }

        public static StringElement BindValue(this StringElement stringElement, IObservable<string> value)
        {
            value.Subscribe(x => stringElement.Value = x);
            return stringElement;
        }

        public static StringElement BindDisclosure(this StringElement stringElement, IObservable<bool> value)
        {
            value.Subscribe(x => stringElement.Accessory = x ? UIKit.UITableViewCellAccessory.DisclosureIndicator : UIKit.UITableViewCellAccessory.None);
            return stringElement;
        }
    }
}

