using System;
using UIKit;
using CodeHub.Core.Services;
using CodeHub.WebViews;
using CodeHub.iOS.ViewComponents;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels;
using ReactiveUI;

namespace CodeHub.iOS.Views.App
{
    public abstract class MarkdownComposerView<TViewModel> : MessageComposerViewController<TViewModel> where TViewModel : class, IComposerViewModel
    {
        private readonly UISegmentedControl _viewSegment;
        private UIWebView _previewView;

        protected MarkdownComposerView(IMarkdownService markdownService)
        {
            TextView.Font = UIFont.SystemFontOfSize(16f);
            TextView.Changed += (sender, e) => ViewModel.Text = TextView.Text;
            TextView.InputAccessoryView = new MarkdownAccessoryView(TextView);

            this.WhenAnyValue(x => x.ViewModel.Text)
                .Subscribe(x => Text = x);

            _viewSegment = new UISegmentedControl(new [] { "Compose", "Preview" });
            _viewSegment.SelectedSegment = 0;
            NavigationItem.TitleView = _viewSegment;
            _viewSegment.ValueChanged += (sender, e) => 
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
                        _previewView = new UIWebView(this.View.Bounds);

                    TextView.RemoveFromSuperview();
                    Add(_previewView);

                    var markdownView = new MarkdownView { Model = markdownService.Convert(TextView.Text) };
                    _previewView.LoadHtmlString(markdownView.GenerateString(), null);
                }
            };
        }
    }
}

