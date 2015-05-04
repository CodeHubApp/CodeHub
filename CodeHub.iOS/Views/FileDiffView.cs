using System;
using CodeHub.Core.ViewModels;
using Foundation;
using UIKit;
using ReactiveUI;
using System.Linq;
using CodeHub.WebViews;

namespace CodeHub.iOS.Views
{
    public abstract class FileDiffView<T> : BaseWebView<T> where T : class, IFileDiffViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.WhenActivated(d => {
                d(this.WhenAnyValue(x => x.ViewModel.Patch, y => y.ViewModel.Comments, (x, y) => Tuple.Create(x, y)).Subscribe(x => {
                    if (x.Item1 == null)
                        LoadContent(string.Empty);
                    else
                    {
                        var comments = (x.Item2 ?? Enumerable.Empty<FileDiffCommentViewModel>())
                            .Select(y => new CommitCommentModel(y.Name, y.AvatarUrl, y.Body, y.Line));
                        var model = new CommitDiffModel(x.Item1.Split('\n'), comments, (int)UIFont.PreferredBody.PointSize);
                        var razorView = new CommitDiffRazorView { Model = model };
                        LoadContent(razorView.GenerateString(), NSBundle.MainBundle.BundlePath);
                    }
                }));
            });
        }

        protected override bool ShouldStartLoad(NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            var url = request.Url;
            if(url.Scheme.Equals("app")) {
                var func = url.Host;

                if(func.Equals("comment") && !string.IsNullOrEmpty(url.Query) && url.Query.StartsWith("line", StringComparison.Ordinal)) 
                {
                    int line;
                    if (int.TryParse(url.Query.Substring(url.Query.IndexOf("=", StringComparison.Ordinal) + 1), out line))
                    {
                        ViewModel.SelectedPatchLine = line;
                        ViewModel.GoToCommentCommand.ExecuteIfCan();
                    }
                }

                return false;
            }

            return base.ShouldStartLoad(request, navigationType);
        }
    }
}

