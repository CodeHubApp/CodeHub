using System;
using ReactiveUI;
using UIKit;
using CoreGraphics;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using CodeHub.WebViews;
using System.Reflection;
using System.IO;
using Splat;
using CodeHub.Core.Services;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core;
using System.Reactive.Threading.Tasks;

namespace CodeHub.iOS.ViewControllers.Settings
{
    public class SyntaxHighlighterViewController : BaseWebViewController
    {
        private static string LoadErrorMessage = "Unable to load code example.";
        const string _resourceName = "CodeHub.iOS.ViewControllers.Settings.SyntaxHighlightExample";
        private readonly UIPickerView _pickerView = new UIPickerView();
        private readonly IApplicationService _applicationService;

        private bool _isModified = false;

        private string _selectedTheme;
        public string SelectedTheme
        {
            get { return _selectedTheme; }
            set { this.RaiseAndSetIfChanged(ref _selectedTheme, value); }
        }

        public SyntaxHighlighterViewController(
            IApplicationService applicationService = null)
            : base(false, false)
        {
            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            _pickerView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            _pickerView.ShowSelectionIndicator = true;
            _pickerView.BackgroundColor = UIColor.FromRGB(244, 244, 244);

            Title = "Syntax Highlighting";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            SelectedTheme = _applicationService.Account.CodeEditTheme ?? "idea";

            var themes = Directory
                .GetFiles(Path.Combine("WebResources", "styles"))
                .Where(x => x.EndsWith(".css", StringComparison.Ordinal))
                .Select(x => Path.GetFileNameWithoutExtension(x))
                .ToArray();

            var model = new PickerModel(themes);
            _pickerView.Model = model;

            var selectedIndex = Array.IndexOf(themes, SelectedTheme);
            if (selectedIndex >= 0 && selectedIndex < themes.Length)
                _pickerView.Select(selectedIndex, 0, false);
            Add(_pickerView);

            var loadCommand = ReactiveCommand.CreateFromTask<string>(LoadTheme);

            loadCommand
                .ThrownExceptions
                .Select(error => new UserError(LoadErrorMessage, error))
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();

            this.WhenAnyValue(x => x.SelectedTheme)
                .InvokeReactiveCommand(loadCommand);

            this.WhenAnyValue(x => x.SelectedTheme)
                .Skip(1)
                .Take(1)
                .Subscribe(_ => _isModified = true);

            OnActivation(d =>
            {
                d(model.SelectedObservable
                  .Subscribe(x => SelectedTheme = x));
            });

            Disappearing
                .Where(_ => _isModified)
                .Select(_ => SelectedTheme)
                .SelectMany(theme => SetSelectedTheme(theme).ToObservable())
                .Subscribe(_ => {}, err => this.Log().ErrorException("Unable to save theme", err));
        }

        private async Task LoadTheme(string theme)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(_resourceName))
            using (var reader = new StreamReader(stream))
            {
                var content = await reader.ReadToEndAsync();
                var zoom = UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Phone;
                var model = new SyntaxHighlighterModel(
                    content, theme ?? "idea", (int)UIFont.PreferredSubheadline.PointSize, zoom, lockWidth: true);
                var razorView = new SyntaxHighlighterWebView { Model = model };
                LoadContent(razorView.GenerateString());
            }
        }

        private async Task SetSelectedTheme(string theme)
        {
            var account = _applicationService.Account;
            account.CodeEditTheme = theme;
            await _applicationService.UpdateActiveAccount();
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

            public IObservable<string> SelectedObservable => _selectedSubject.AsObservable();

            public PickerModel(IEnumerable<string> values)
            {
                _values = values.ToArray();   
            }

            public override nint GetComponentCount(UIPickerView picker) => 1;

            public override void Selected(UIPickerView picker, nint row, nint component) => _selectedSubject.OnNext(_values[row]);

            public override nint GetRowsInComponent(UIPickerView picker, nint component) => _values.Length;

            public override string GetTitle(UIPickerView picker, nint row, nint component) => _values[row];
        }
    }
}

