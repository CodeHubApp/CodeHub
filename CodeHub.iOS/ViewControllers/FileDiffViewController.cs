using System;
using CodeHub.Core.ViewModels;
using Foundation;
using UIKit;
using ReactiveUI;
using System.Linq;
using CodeHub.WebViews;
using Splat;

namespace CodeHub.iOS.ViewControllers
{
    public abstract class FileDiffViewController<T> : BaseWebViewController<T>
        where T : class, IFileDiffViewModel
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

                try
                {
                    if(func.Equals("comment") && !string.IsNullOrEmpty(url.Query))
                    {
                        var param = url.Query.Split('&');
                        var p = param.ToDictionary(x => x.Split('=')[0], x => x.Split('=')[1]);
                        ViewModel.SelectedPatchLine = Tuple.Create(int.Parse(p["index"]), int.Parse(p["line"]));
                        ViewModel.GoToCommentCommand.ExecuteIfCan();
                    }
                }
                catch (Exception e)
                {
                    this.Log().ErrorException("Unable to comment on diff", e);
                }

                return false;
            }

            return base.ShouldStartLoad(request, navigationType);
        }
    }
}

