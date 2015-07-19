using System;
using UIKit;
using CodeHub.Core.Services;
using CodeHub.WebViews;
using CodeHub.iOS.ViewControllers;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.Views;
using ReactiveUI;

namespace CodeHub.iOS.ViewControllers.App
{
    public abstract class MarkdownComposerViewController<TViewModel> : MessageComposerViewController<TViewModel> where TViewModel : class, IComposerViewModel
    {
        private readonly UISegmentedControl _viewSegment;
        private UIWebView _previewView;

        protected MarkdownComposerViewController(IMarkdownService markdownService)
        {
            TextView.Font = UIFont.PreferredBody;
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

                    var markdown = markdownService.Convert(TextView.Text);
                    var model = new DescriptionModel(markdown, (int)UIFont.PreferredSubheadline.PointSize);
                    var markdownView = new MarkdownView { Model = model };
                    _previewView.LoadHtmlString(markdownView.GenerateString(), null);
                }
            };
        }
    }
}

