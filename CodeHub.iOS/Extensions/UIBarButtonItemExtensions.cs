using ReactiveUI;

namespace MonoTouch.UIKit
{
    public static class UIBarButtonItemExtensions
    {
        public static UIBarButtonItem WithCommand(this UIBarButtonItem @this, IReactiveCommand command)
        {
            @this.Clicked += (sender, e) => command.ExecuteIfCan();
            @this.EnableIfExecutable(command.CanExecuteObservable);
            return @this;
        }
    }
}

