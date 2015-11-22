using System;
using ReactiveUI;

namespace CodeHub.iOS.DialogElements
{
    public static class ElementExtensions
    {
        public static IDisposable BindCommand(this StringElement stringElement, IReactiveCommand<object> cmd)
        {
            return stringElement.Clicked.InvokeCommand(cmd);
        }

        public static IDisposable BindCaption(this Element stringElement, IObservable<string> caption)
        {
            return caption.Subscribe(x => stringElement.Caption = x);
        }

        public static IDisposable BindValue(this StringElement stringElement, IObservable<string> value)
        {
            return value.Subscribe(x => stringElement.Value = x);
        }

        public static IDisposable BindDisclosure(this StringElement stringElement, IObservable<bool> value)
        {
            return value.Subscribe(x => stringElement.Accessory = x ? UIKit.UITableViewCellAccessory.DisclosureIndicator : UIKit.UITableViewCellAccessory.None);
        }
    }
}

