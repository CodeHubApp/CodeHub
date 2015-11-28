using System;
using ReactiveUI;
using System.Reactive.Disposables;
using UIKit;

namespace CodeHub.iOS.DialogElements
{
    public static class ElementExtensions
    {
        public static IDisposable BindCommand<T>(this StringElement stringElement, IReactiveCommand<T> cmd)
        {
            var d1 = stringElement.Clicked.InvokeCommand(cmd);
            var d2 = cmd.CanExecuteObservable.Subscribe(x => stringElement.SelectionStyle = (x ? null : (UITableViewCellSelectionStyle?)UITableViewCellSelectionStyle.None));
            return new CompositeDisposable(d1, d2);
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
            return value.SubscribeSafe(x => stringElement.Accessory = x ? UIKit.UITableViewCellAccessory.DisclosureIndicator : UIKit.UITableViewCellAccessory.None);
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

