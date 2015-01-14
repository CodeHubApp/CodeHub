using System;
using System.Collections.Generic;
using CodeHub.Core.ViewModels.Source;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using CodeHub.Core.Services;
using ReactiveUI;
using CodeHub.WebViews;
using System.IO;

namespace CodeHub.iOS.Views.Source
{
    public class ChangesetDiffView : BaseWebView<ChangesetDiffViewModel>
    {
        public ChangesetDiffView(INetworkActivityService networkActivityService)
            : base(networkActivityService)
        {
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            this.WhenAnyValue(x => x.ViewModel.Patch).Subscribe(x =>
            {
                if (x == null)
                    LoadContent(string.Empty);
                else
                {
                    var razorView = new CommitDiffRazorView
                    { 
                        Model = x.Split('\n')
                    };

                    string contentDirectoryPath = Path.Combine (NSBundle.MainBundle.BundlePath, "WebViews");
                    LoadContent(razorView.GenerateString(), contentDirectoryPath);
                }
            });
//
//
//			ViewModel.Bind(x => x.FilePath, x =>
//			{
//				var data = System.IO.File.ReadAllText(x, System.Text.Encoding.UTF8);
//				var patch = JavaScriptStringEncode(data);
//				ExecuteJavascript("var a = \"" + patch + "\"; patch(a);");
//			});
//
//			ViewModel.BindCollection(x => x.Comments, e =>
//			{
//				//Convert it to something light weight
//				var slimComments = ViewModel.Comments.Items.Where(x => string.Equals(x.Path, ViewModel.Filename)).Select(x => new { 
//					Id = x.Id, User = x.User.Login, Avatar = x.User.AvatarUrl, LineTo = x.Position, LineFrom = x.Position,
//					Content = x.Body, Date = x.UpdatedAt
//				}).ToList();
//
//				var c = _serializationService.Serialize(slimComments);
//				ExecuteJavascript("var a = " + c + "; setComments(a);");
//			});
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
					//var commentModel = _serializationService.Deserialize<JavascriptCommentModel>(UrlDecode(url.Fragment));
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
    }
}

