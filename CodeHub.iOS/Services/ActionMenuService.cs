using System;
using CodeHub.Core.Services;
using MonoTouch.UIKit;
using System.Collections.Generic;
using ReactiveUI;
using System.Threading.Tasks;
using Xamarin.Utilities.Views;
using System.Linq;

namespace CodeHub.iOS.Services
{
    public class ActionMenuService : IActionMenuService
    {
        public IActionMenu Create(string title)
        {
            return new ActionMenu(title, UIApplication.SharedApplication.Delegate.Window);
        }

        public IPickerMenu CreatePicker()
        {
            return new PickerMenu();
        }

        private class ActionMenu : IActionMenu
        {
            private readonly string _title;
            private readonly UIWindow _window;
            private readonly IList<Tuple<string, IReactiveCommand>> _buttonActions = new List<Tuple<string, IReactiveCommand>>();

            public ActionMenu(string title, UIWindow window)
            {
                _title = title;
                _window = window;
            }

            public void AddButton(string title, IReactiveCommand command)
            {
                _buttonActions.Add(Tuple.Create(title, command));
            }

            public Task Show()
            {
                var a = new TaskCompletionSource<object>();
                var buttonMap = new Dictionary<int, IReactiveCommand>();

                var actionSheet = new UIActionSheet(_title);
                foreach (var b in _buttonActions)
                {
                    var index = actionSheet.AddButton(b.Item1);
                    buttonMap.Add(index, b.Item2);
                }

                actionSheet.Clicked += (s, e) =>
                {
                    if (buttonMap.ContainsKey(e.ButtonIndex))
                        buttonMap[e.ButtonIndex].ExecuteIfCan();
                    a.SetResult(true);
                };

                var cancelButton = actionSheet.AddButton("Cancel");
                actionSheet.CancelButtonIndex = cancelButton;
                actionSheet.DismissWithClickedButtonIndex(cancelButton, true);
                actionSheet.ShowInView(_window);
                return a.Task;
            }
        }

        private class PickerMenu : IPickerMenu
        {
            private readonly LinkedList<string> _options = new LinkedList<string>();

            public ICollection<string> Options
            {
                get { return _options; }
            }

            public int SelectedOption { get; set; }

            public Task<int> Show()
            {
                var a = new TaskCompletionSource<int>();

                new PickerAlertView(_options.ToArray(), SelectedOption, x =>
                {
                    if (x < _options.Count)
                        a.SetResult(x);
                }).Show();

                return a.Task;
            }
        }
    }
}

