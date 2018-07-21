using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodeHub.Core.Services;
using CodeHub.iOS.Views;
using CodeHub.WebViews;
using Splat;
using UIKit;

namespace CodeHub.iOS.ViewControllers
{
    public class MarkdownComposerViewController : SegmentViewController
    {
        private readonly UIBarButtonItem _saveButton = new UIBarButtonItem(UIBarButtonSystemItem.Save);
        private readonly IMarkdownService _markdownService;
        private readonly TextViewController _textViewController;
        private readonly Lazy<WebViewController> _previewViewController;

        public IObservable<Unit> Saved => _saveButton.GetClickedObservable().Select(_ => Unit.Default);

        public bool EnableSendButton
        {
            get => _saveButton.Enabled;
            set => _saveButton.Enabled = value;
        }

        public string Text
        {
            get => _textViewController.Text;
            set => _textViewController.Text = value;
        }

        public MarkdownComposerViewController(
            IMarkdownService markdownService = null)
            : base(new [] { "Compose", "Preview" })
        {
            _markdownService = markdownService ?? Locator.Current.GetService<IMarkdownService>();
            _textViewController = new TextViewController();
            _previewViewController = new Lazy<WebViewController>();

            _textViewController.TextView.InputAccessoryView =
                new MarkdownAccessoryView(_textViewController.TextView);
        }

        protected override void SegmentValueChanged(int id)
        {
            if (id == 0)
            {
                AddTable(_textViewController);

                if (_previewViewController.IsValueCreated)
                    RemoveIfLoaded(_previewViewController.Value);

                _textViewController.TextView.BecomeFirstResponder();
            }
            else
            {
                AddTable(_previewViewController.Value);
                RemoveIfLoaded(_textViewController);
                LoadPreview().ToBackground();
            }
        }

        private async Task LoadPreview()
        {
            var markdownText = await _markdownService.Convert(_textViewController.Text);
            var model = new MarkdownModel(markdownText, (int)UIFont.PreferredSubheadline.PointSize);
            var view = new MarkdownWebView { Model = model }.GenerateString();
            _previewViewController.Value.LoadContent(view);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationItem.RightBarButtonItem = _saveButton;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            NavigationItem.RightBarButtonItem = null;
        }
    }
}

