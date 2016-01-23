using System;
using ReactiveUI;
using UIKit;

namespace CodeHub.iOS.DialogElements
{
    public static class ElementExtensions
    {
        public static IDisposable BindCommand<T>(this StringElement stringElement, IReactiveCommand<T> cmd)
        {
            return stringElement.Clicked.InvokeCommand(cmd);
        }

        public static IDisposable BindCaption<T>(this Element stringElement, IObservable<T> caption)
        {
            return caption.SubscribeSafe(x => stringElement.Caption = x.ToString());
        }

        public static IDisposable BindValue<T>(this StringElement stringElement, IObservable<T> value)
        {
            return value.SubscribeSafe(x => stringElement.Value = x.ToString());
        }

        public static IDisposable BindDisclosure(this StringElement stringElement, IObservable<bool> value)
        {
            return value.SubscribeSafe(x => {
                stringElement.SelectionStyle = x ? UITableViewCellSelectionStyle.Blue : UITableViewCellSelectionStyle.None;
                stringElement.Accessory = x ? UITableViewCellAccessory.DisclosureIndicator : UITableViewCellAccessory.None;
            });
        }

        public static IDisposable BindText<T>(this SplitButtonElement.SplitButton button, IObservable<T> value)
        {
            return value.SubscribeSafe(x => button.Text = x.ToString());
        }

        public static IDisposable BindText<T>(this SplitViewElement.SplitButton button, IObservable<T> value)
        {
            return value.SubscribeSafe(x => button.Text = x.ToString());
        }
    }
}

