using System;
using UIKit;
using System.Collections.Generic;
using ReactiveUI;
using System.Threading.Tasks;
using System.Linq;
using Foundation;
using CodeHub.Core.Factories;
using CodeHub.iOS.ViewComponents;
using System.Diagnostics;

namespace CodeHub.iOS.Factories
{
    public class ActionMenuFactory : IActionMenuFactory
    {
        public IActionMenu Create(string title)
        {
            return new ActionMenu(title);
        }

        public IPickerMenu CreatePicker()
        {
            return new PickerMenu();
        }

        public Task ShareUrl(Uri uri)
        {
            var item = new NSUrl(uri.AbsoluteUri);
            var activityItems = new NSObject[] { item };
            UIActivity[] applicationActivities = null;
            var activityController = new UIActivityViewController (activityItems, applicationActivities);
            var viewController = UIApplication.SharedApplication.KeyWindow.RootViewController;
            return viewController.PresentViewControllerAsync(activityController, true);
        }

        public void SendToPasteBoard(string str)
        {
            UIPasteboard.General.String = str;
        }

        private class ActionMenu : IActionMenu
        {
            private readonly string _title;
            private readonly IList<Tuple<string, IReactiveCommand>> _buttonActions = new List<Tuple<string, IReactiveCommand>>();

            public ActionMenu(string title)
            {
                _title = title;
            }

            public void AddButton(string title, IReactiveCommand command)
            {
                _buttonActions.Add(Tuple.Create(title, command));
            }

            public Task Show(object sender)
            {
                var a = new TaskCompletionSource<object>();
                var sheet =  UIAlertController.Create(_title, null, UIAlertControllerStyle.ActionSheet);

                foreach (var b in _buttonActions)
                {
                    sheet.AddAction(UIAlertAction.Create(b.Item1, UIAlertActionStyle.Default, x => {
                        b.Item2.ExecuteIfCan();
                        a.SetResult(true);
                    }));
                }

                sheet.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, x => a.SetResult(true)));

                var viewController = UIApplication.SharedApplication.KeyWindow.GetVisibleViewController();

                (sender as UIBarButtonItem).Do(x => sheet.PopoverPresentationController.BarButtonItem = x);
                (sender as UIView).Do(x => sheet.PopoverPresentationController.SourceView = x);

                // Last resort
                if (sheet.PopoverPresentationController.SourceView == null 
                    && sheet.PopoverPresentationController.BarButtonItem == null)
                {
                    Debugger.Break();
                    sheet.PopoverPresentationController.SourceView = viewController.View;
                }

                viewController.PresentViewController(sheet, true, null);
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

            public Task<int> Show(object sender)
            {
                var a = new TaskCompletionSource<int>();

                new PickerAlertView(_options.ToArray(), SelectedOption, x =>
                {
                    if (x < _options.Count)
                        a.SetResult((int)x);
                }).Show();

                return a.Task;
            }
        }
    }
}

