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

namespace CodeHub.iOS.Views.Source
{
    public class ChangesetDiffView : BaseWebView<ChangesetDiffViewModel>
    {
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            this.WhenAnyValue(x => x.ViewModel.Patch)
                .CombineLatest(this.WhenAnyObservable(x => x.ViewModel.Comments.Changed), (x, y) => x)
                .Subscribe(x => {
                    if (x == null)
                        LoadContent(string.Empty);
                    else
                    {
                        var comments = ViewModel.Comments
                            .Where(y => y.Position.HasValue)
                            .OrderBy(y => y.Id)
                            .Select(y => new CommitCommentModel(y.User.Login, y.User.AvatarUrl, y.Body, y.Position.Value));
                        var model = new CommitDiffModel(x.Split('\n'), comments, (int)UIFont.PreferredBody.PointSize);
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
					//PromptForComment(commentModel);
                }

				return false;
            }

            return base.ShouldStartLoad(request, navigationType);
        }


        private void ShowCommentComposer(int line)
        {
//            var composer = new MarkdownComposerViewController();
//			composer.NewComment(this, async (text) => {
//				try
//				{
//					await composer.DoWorkAsync("Commenting...", () => ViewModel.PostComment(text, line));
//					composer.CloseComposer();
//				}
//				catch (Exception e)
//				{
//					MonoTouch.Utilities.ShowAlert("Unable to Comment", e.Message);
//					composer.EnableSendButton = true;
//				}
//            });
        }

        public class CommentView : MarkdownComposerView<ChangesetCommentViewModel>, IModalView
        {
            public CommentView(IMarkdownService markdownService, IAlertDialogFactory alertDialogFactory) 
                : base(markdownService)
            {
                this.WhenAnyValue(x => x.ViewModel.SaveCommand)
                    .Select(x => x.ToBarButtonItem(UIBarButtonSystemItem.Save))
                    .Subscribe(x => NavigationItem.RightBarButtonItem = x);

                this.WhenAnyValue(x => x.ViewModel.DismissCommand)
                    .Select(x => x.ToBarButtonItem(Images.Cancel))
                    .Subscribe(x => NavigationItem.LeftBarButtonItem = x);

                ViewModel = new ChangesetCommentViewModel(alertDialogFactory, null);
            }
        }
    }
}

