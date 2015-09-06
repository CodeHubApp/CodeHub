using System;
using CodeHub.iOS.Views.Source;
using UIKit;
using Foundation;
using GitHubSharp.Models;
using System.Collections.Generic;
using System.Linq;
using CodeFramework.ViewControllers;
using CodeHub.Core.ViewModels.Source;
using CodeFramework.iOS.ViewControllers;
using CodeFramework.iOS.Utils;
using CodeFramework.Core.Services;
using CodeHub.iOS.ViewControllers;

namespace CodeHub.ViewControllers
{
	public class ChangesetDiffView : FileSourceView
    {
		private readonly IJsonSerializationService _serializationService = Cirrious.CrossCore.Mvx.Resolve<IJsonSerializationService>();
		private bool _domLoaded = false;
		private List<string> _toBeExecuted = new List<string>();

		public new ChangesetDiffViewModel ViewModel
		{
			get { return (ChangesetDiffViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();


			ViewModel.Bind(x => x.FilePath, x =>
			{
				var data = System.IO.File.ReadAllText(x, System.Text.Encoding.UTF8);
				var patch = JavaScriptStringEncode(data);
				ExecuteJavascript("var a = \"" + patch + "\"; patch(a);");
			});

			ViewModel.BindCollection(x => x.Comments, e =>
			{
				//Convert it to something light weight
				var slimComments = ViewModel.Comments.Items.Where(x => string.Equals(x.Path, ViewModel.Filename)).Select(x => new { 
					Id = x.Id, User = x.User.Login, Avatar = x.User.AvatarUrl, LineTo = x.Position, LineFrom = x.Position,
					Content = x.Body, Date = x.UpdatedAt
				}).ToList();

				var c = _serializationService.Serialize(slimComments);
				ExecuteJavascript("var a = " + c + "; setComments(a);");
			});
		}

		private bool _isLoaded;
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			if (!_isLoaded)
			{
				var path = System.IO.Path.Combine(NSBundle.MainBundle.BundlePath, "Diff", "diffindex.html");
				var uri = Uri.EscapeUriString("file://" + path) + "#" + Environment.TickCount;
				Web.LoadRequest(new Foundation.NSUrlRequest(new Foundation.NSUrl(uri)));
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
					var commentModel = _serializationService.Deserialize<JavascriptCommentModel>(UrlDecode(url.Fragment));
					PromptForComment(commentModel);
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
            title = "Line ".t() + model.FileLine;

            var sheet = MonoTouch.Utilities.GetSheet(title);
            var addButton = sheet.AddButton("Add Comment".t());
            var cancelButton = sheet.AddButton("Cancel".t());
            sheet.CancelButtonIndex = cancelButton;
            sheet.DismissWithClickedButtonIndex(cancelButton, true);
			sheet.Dismissed += (sender, e) => {
				BeginInvokeOnMainThread(() =>
					{
                if (e.ButtonIndex == addButton)
                    ShowCommentComposer(model.PatchLine);
					});
            };

            sheet.ShowInView(this.View);
        }

        private void ShowCommentComposer(int line)
        {
            var composer = new MarkdownComposerViewController();
			composer.NewComment(this, async (text) => {
				try
				{
					await composer.DoWorkAsync("Commenting...", () => ViewModel.PostComment(text, line));
					composer.CloseComposer();
				}
				catch (Exception e)
				{
					MonoTouch.Utilities.ShowAlert("Unable to Comment".t(), e.Message);
					composer.EnableSendButton = true;
				}
            });
        }
    }
}

