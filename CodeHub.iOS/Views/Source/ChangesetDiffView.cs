using System;
using System.Collections.Generic;
using CodeHub.Core.ViewModels.Source;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using CodeHub.Core.Services;
using Xamarin.Utilities.Services;
using Xamarin.Utilities.Factories;

namespace CodeHub.iOS.Views.Source
{
	public class ChangesetDiffView : FileSourceView<ChangesetDiffViewModel>
    {
		private bool _domLoaded = false;
		private List<string> _toBeExecuted = new List<string>();
	    private UIActionSheet _actionSheet;

        public ChangesetDiffView(INetworkActivityService networkActivityService, IAlertDialogFactory alertDialogFactory)
            : base(networkActivityService, alertDialogFactory)
        {
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
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

		private bool _isLoaded;
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			if (!_isLoaded)
			{
				var path = System.IO.Path.Combine(NSBundle.MainBundle.BundlePath, "Diff", "diffindex.html");
				var uri = Uri.EscapeUriString("file://" + path) + "#" + Environment.TickCount;
				Web.LoadRequest(new MonoTouch.Foundation.NSUrlRequest(new MonoTouch.Foundation.NSUrl(uri)));
				_isLoaded = true;
			}
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

				if (func.Equals("ready"))
				{
					_domLoaded = true;
					foreach (var e in _toBeExecuted)
						Web.EvaluateJavascript(e);
				}
				else if(func.Equals("comment")) 
				{
					//var commentModel = _serializationService.Deserialize<JavascriptCommentModel>(UrlDecode(url.Fragment));
					//PromptForComment(commentModel);
                }

				return false;
            }

            return base.ShouldStartLoad(request, navigationType);
        }

		private void ExecuteJavascript(string data)
		{
			if (_domLoaded)
				InvokeOnMainThread(() => Web.EvaluateJavascript(data));
			else
				_toBeExecuted.Add(data);
		}

        private void PromptForComment(JavascriptCommentModel model)
        {
            string title = string.Empty;
            title = "Line " + model.FileLine;

            var sheet = _actionSheet = new UIActionSheet(title);
            var addButton = sheet.AddButton("Add Comment");
            var cancelButton = sheet.AddButton("Cancel");
            sheet.CancelButtonIndex = cancelButton;
            sheet.DismissWithClickedButtonIndex(cancelButton, true);
            sheet.Clicked += (sender, e) => {
                if (e.ButtonIndex == addButton)
                    ShowCommentComposer(model.PatchLine);
                _actionSheet = null;
            };

            sheet.ShowInView(this.View);
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

