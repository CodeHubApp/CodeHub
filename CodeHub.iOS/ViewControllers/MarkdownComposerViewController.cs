using System;
using UIKit;
using MvvmCross.Platform;
using CodeHub.Core.Services;
using Foundation;
using CodeHub.WebViews;
using WebKit;
using CodeHub.iOS.Views;
using ReactiveUI;

namespace CodeHub.iOS.ViewControllers
{
    public class MarkdownComposerViewController : Composer
    {
        private readonly UISegmentedControl _viewSegment;
        private WKWebView _previewView;

        public MarkdownComposerViewController()
        {
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

                var markdownService = Mvx.Resolve<IMarkdownService>();
                var markdownText = markdownService.Convert(Text);
                var model = new MarkdownModel(markdownText, (int)UIFont.PreferredSubheadline.PointSize);
                var view = new MarkdownWebView { Model = model }.GenerateString();
                _previewView.LoadHtmlString(view, NSBundle.MainBundle.BundleUrl);
            }
        }
    }
}

