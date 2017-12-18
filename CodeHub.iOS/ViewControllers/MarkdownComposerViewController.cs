using System;
using UIKit;
using CodeHub.Core.Services;
using Foundation;
using CodeHub.WebViews;
using WebKit;
using CodeHub.iOS.Views;
using ReactiveUI;
using System.Threading.Tasks;
using Splat;

namespace CodeHub.iOS.ViewControllers
{
    public class MarkdownComposerViewController : Composer
    {
        private readonly UISegmentedControl _viewSegment;
        private readonly IMarkdownService _markdownService;
        private WKWebView _previewView;

        public MarkdownComposerViewController(IMarkdownService markdownService = null)
        {
            _markdownService = markdownService ?? Locator.Current.GetService<IMarkdownService>();

            _viewSegment = new UISegmentedControl(new[] { "Compose", "Preview" })
            {
                SelectedSegment = 0
            };

            NavigationItem.TitleView = _viewSegment;

            TextView.InputAccessoryView = new MarkdownAccessoryView(TextView);

            this.WhenActivated(d =>
            {
                d(_viewSegment.GetChangedObservable()
                  .Subscribe(_ => SegmentValueChanged()));
            });
        }

        void SegmentValueChanged()
        {
            if (_viewSegment.SelectedSegment == 0)
            {
                if (_previewView != null)
                {
                    _previewView.RemoveFromSuperview();
                    _previewView.Dispose();
                    _previewView = null;
                }

                Add(TextView);
                TextView.BecomeFirstResponder();
            }
            else
            {
                if (_previewView == null)
                    _previewView = new WKWebView(this.View.Bounds, new WKWebViewConfiguration());

                TextView.RemoveFromSuperview();
                Add(_previewView);

                LoadPreview(_previewView).ToBackground();
            }
        }

        private async Task LoadPreview(WKWebView previewView)
        {
            var markdownText = await _markdownService.Convert(Text);
            var model = new MarkdownModel(markdownText, (int)UIFont.PreferredSubheadline.PointSize);
            var view = new MarkdownWebView { Model = model }.GenerateString();
            previewView.LoadHtmlString(view, NSBundle.MainBundle.BundleUrl);
        }
    }
}

