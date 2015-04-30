using System;
using CodeHub.Core.ViewModels.Source;
using Foundation;
using UIKit;
using CodeHub.Core.Services;
using ReactiveUI;
using CodeHub.WebViews;
using CodeHub.iOS.Views.App;
using System.Reactive.Linq;
using CodeHub.Core.Factories;
using System.Linq;
using CodeHub.Core.ViewModels.PullRequests;
using System.Collections.ObjectModel;

namespace CodeHub.iOS.Views.PullRequests
{
    public class PullRequestDiffView : BaseWebView<PullRequestDiffViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.WhenAnyValue(x => x.ViewModel.Patch, y => y.ViewModel.Comments, (x, y) => Tuple.Create(x, y))
                .Subscribe(x => {
                    if (x.Item1 == null)
                        LoadContent(string.Empty);
                    else
                    {
                        var comments = (x.Item2 ?? Enumerable.Empty<Octokit.PullRequestReviewComment>())
                            .Where(y => y.Position.HasValue)
                            .Select(y => new CommitCommentModel(y.User.Login, y.User.AvatarUrl, y.Body, y.Position.Value));
                        var model = new CommitDiffModel(x.Item1.Split('\n'), comments, (int)UIFont.PreferredBody.PointSize);
                        var razorView = new CommitDiffRazorView { Model = model };
                        LoadContent(razorView.GenerateString(), NSBundle.MainBundle.BundlePath);
                    }
                });
        }

        private class JavascriptCommentModel
        {
            public int PatchLine { get; set; }
            public int FileLine { get; set; }
        }

        protected override bool ShouldStartLoad(NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            var url = request.Url;
            if(url.Scheme.Equals("app")) {
                var func = url.Host;

                if(func.Equals("comment")) 
                {
                    var commentModel = Newtonsoft.Json.JsonConvert.DeserializeObject<JavascriptCommentModel>(UrlDecode(url.Fragment));
                    ViewModel.SelectedPatchLine = commentModel.PatchLine;
                    ViewModel.GoToCommentCommand.ExecuteIfCan();
                }

                return false;
            }

            return base.ShouldStartLoad(request, navigationType);
        }


        private void ShowCommentComposer(int line)
        {
            //            var composer = new MarkdownComposerViewController();
            //          composer.NewComment(this, async (text) => {
            //              try
            //              {
            //                  await composer.DoWorkAsync("Commenting...", () => ViewModel.PostComment(text, line));
            //                  composer.CloseComposer();
            //              }
            //              catch (Exception e)
            //              {
            //                  MonoTouch.Utilities.ShowAlert("Unable to Comment", e.Message);
            //                  composer.EnableSendButton = true;
            //              }
            //            });
        }

    }
}

