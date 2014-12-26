using System;
using ReactiveUI;
using MonoTouch.UIKit;
using CodeHub.Core.ViewModels.Settings;
using Xamarin.Utilities.ViewControllers;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using Xamarin.Utilities.Services;
using CodeHub.WebViews;
using System.Reflection;
using System.IO;
using Xamarin.Utilities.Factories;
using Splat;

namespace CodeHub.iOS.Views.Settings
{
    public class SyntaxHighlighterSettingsView : ReactiveWebViewController<SyntaxHighlighterSettingsViewModel>, IEnableLogger
    {
        const string _resourceName = "CodeHub.iOS.Views.Settings.SyntaxHighlightExample";
        private readonly UIPickerView _pickerView = new UIPickerView();
        private readonly IAlertDialogFactory _alertDialogFactory;

        public SyntaxHighlighterSettingsView(INetworkActivityService networkActivity, IAlertDialogFactory alertDialogFactory)
            : base(networkActivity)
        {
            _alertDialogFactory = alertDialogFactory;
            _pickerView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            _pickerView.ShowSelectionIndicator = true;
            _pickerView.BackgroundColor = UIColor.FromRGB(244, 244, 244);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var themes = ViewModel.Themes.ToArray();
            var model = new PickerModel(themes);
            _pickerView.Model = model;
            Add(_pickerView);

            var selectedIndex = Array.IndexOf(themes, ViewModel.SelectedTheme);
            if (selectedIndex >= 0 && selectedIndex < themes.Length)
                _pickerView.Select(selectedIndex, 0, false);

            this.WhenActivated(d =>
            {
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
                    var razorView = new SyntaxHighlighterView
                    { 
                        Model = new SourceBrowserModel
                        {
                            Content = reader.ReadToEnd(),
                            Theme = theme ?? "idea"
                        }
                    };

                    base.LoadContent(razorView.GenerateString());
                }
            }
            catch (Exception e)
            {
                this.Log().ErrorException("Unable to load Syntax Highlighter", e);
                _alertDialogFactory.Alert("Unable to load example!", "Can't load the example code. Looks like you're on your own...");
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            ViewModel.SaveCommand.ExecuteIfCan();
        }

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
            Web.Frame = new RectangleF(0, 0, View.Bounds.Width, View.Bounds.Height - _pickerView.Frame.Height);
            _pickerView.Frame = new RectangleF(0, View.Bounds.Height - _pickerView.Frame.Height, View.Bounds.Width, _pickerView.Frame.Height);
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

            public override int GetComponentCount(UIPickerView picker)
            {
                return 1;
            }

            public override void Selected(UIPickerView picker, int row, int component)
            {
                _selectedSubject.OnNext(_values[row]);
            }

            public override int GetRowsInComponent(UIPickerView picker, int component)
            {
                return _values.Length;
            }

            public override string GetTitle(UIPickerView picker, int row, int component)
            {

                return _values[row];
            }
        }
    }
}

