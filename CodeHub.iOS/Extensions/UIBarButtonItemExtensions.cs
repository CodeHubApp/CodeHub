using System;
using ReactiveUI;

namespace MonoTouch.UIKit
{
    public static class UIBarButtonItemExtensions
    {
        public static void EnableIfExecutable(this UIBarButtonItem @this, IObservable<bool> observable)
        {
            observable.Subscribe(x => @this.Enabled = x);
        }

        public static void EnableIfExecutable(this UIBarButtonItem @this, IReactiveCommand command)
        {
            EnableIfExecutable(@this, command.CanExecuteObservable);
        }

        public static UIBarButtonItem WithCommand(this UIBarButtonItem @this, IReactiveCommand command)
        {
            @this.Clicked += (sender, e) => command.ExecuteIfCan();
            @this.EnableIfExecutable(command.CanExecuteObservable);
            return @this;
        }
    }
}