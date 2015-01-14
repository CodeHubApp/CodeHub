using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using ReactiveUI;
using System.Threading.Tasks;
using System.Linq;
using MonoTouch.Foundation;
using CodeHub.Core.Factories;
using CodeHub.iOS.ViewComponents;

namespace CodeHub.iOS.Factories
{
    public class ActionMenuFactory : IActionMenuFactory
    {
        public IActionMenu Create(string title)
        {
            return new ActionMenu(title, UIApplication.SharedApplication.KeyWindow);
        }

        public IPickerMenu CreatePicker()
        {
            return new PickerMenu();
        }

        public Task ShareUrl(string url)
        {
            var item = new NSUrl(url);
            var activityItems = new NSObject[] { item };
            UIActivity[] applicationActivities = null;
            var activityController = new UIActivityViewController (activityItems, applicationActivities);
            var topViewController = UIApplication.SharedApplication.KeyWindow.GetVisibleViewController();
            return topViewController.PresentViewControllerAsync(activityController, true);
        }

        public void SendToPasteBoard(string str)
        {
            UIPasteboard.General.String = str;
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
                };

                actionSheet.Dismissed += (sender, e) => a.SetResult(true);
                actionSheet.CancelButtonIndex = actionSheet.AddButton("Cancel");
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

