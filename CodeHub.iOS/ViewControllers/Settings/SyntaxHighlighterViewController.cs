using System;
using ReactiveUI;
using UIKit;
using CodeHub.Core.ViewModels.Settings;
using CoreGraphics;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using CodeHub.WebViews;
using System.Reflection;
using System.IO;
using Splat;
using CodeHub.Core.Factories;

namespace CodeHub.iOS.ViewControllers.Settings
{
    public class SyntaxHighlighterViewController : BaseWebViewController<SyntaxHighlighterViewModel>
    {
        const string _resourceName = "CodeHub.iOS.ViewControllers.Settings.SyntaxHighlightExample";
        private readonly UIPickerView _pickerView = new UIPickerView();
        private readonly IAlertDialogFactory _alertDialogFactory;

        public SyntaxHighlighterViewController(IAlertDialogFactory alertDialogFactory)
        {
            _alertDialogFactory = alertDialogFactory;
            _pickerView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            _pickerView.ShowSelectionIndicator = true;
            _pickerView.BackgroundColor = UIColor.FromRGB(244, 244, 244);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Disappearing.InvokeCommand(ViewModel.SaveCommand);

            var themes = ViewModel.Themes.ToArray();
            var model = new PickerModel(themes);
            _pickerView.Model = model;

            var selectedIndex = Array.IndexOf(themes, ViewModel.SelectedTheme);
            if (selectedIndex >= 0 && selectedIndex < themes.Length)
                _pickerView.Select(selectedIndex, 0, false);
            Add(_pickerView);

            OnActivation(d => {
                d(model.SelectedObservable.Subscribe(x => ViewModel.SelectedTheme = x));
                d(this.WhenAnyValue(x => x.ViewModel.SelectedTheme).Subscribe(LoadContent));
            });
        }

        private new void LoadContent(string theme)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream(_resourceName))
                using (var reader = new StreamReader(stream))
                {
                    var model = new SourceBrowserModel(reader.ReadToEnd(), theme ?? "idea", (int)UIFont.PreferredSubheadline.PointSize, _resourceName);
                    var razorView = new SyntaxHighlighterView { Model = model };
                    base.LoadContent(razorView.GenerateString());
                }
            }
            catch (Exception e)
            {
                this.Log().ErrorException("Unable to load Syntax Highlighter", e);
                _alertDialogFactory.Alert("Unable to load example!", "Can't load the example code. Looks like you're on your own...");
            }
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
            Web.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height - _pickerView.Frame.Height);
            _pickerView.Frame = new CGRect(0, View.Bounds.Height - _pickerView.Frame.Height, View.Bounds.Width, _pickerView.Frame.Height);
        }

        private class PickerModel : UIPickerViewModel
        {
            private readonly string[] _values;
            private readonly ISubject<string> _selectedSubject = new Subject<string>();

            public IObservable<string> SelectedObservable
            {
                get { return _selectedSubject; }
            }

            public PickerModel(IEnumerable<string> values)
            {
                _values = values.ToArray();
            }

            public override nint GetComponentCount(UIPickerView picker)
            {
                return 1;
            }

            public override void Selected(UIPickerView picker, nint row, nint component)
            {
                _selectedSubject.OnNext(_values[row]);
            }

            public override nint GetRowsInComponent(UIPickerView picker, nint component)
            {
                return _values.Length;
            }

            public override string GetTitle(UIPickerView picker, nint row, nint component)
            {
                return _values[row];
            }
        }
    }
}

