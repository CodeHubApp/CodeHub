using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using GitHubSharp.Models;
using System.Collections.Generic;
using System.Linq;
using CodeFramework.Controllers;

namespace CodeHub.ViewControllers
{
    public class ChangesetDiffViewController : FileSourceViewController
    {
        private readonly string _parent;
        private readonly string _user;
        private readonly string _slug;
        private readonly string _branch;
        private readonly string _path;
        private string _baseText, _newText;
        public bool Removed { get; set; }
        public bool Added { get; set; }
        public List<CommentModel> Comments;

        public ChangesetDiffViewController(string user, string slug, string branch, string parent, string path)
        {
            _parent = parent;
            _user = user;
            _slug = slug;
            _branch = branch;
            _path = path;

            //Create the filename
            var fileName = System.IO.Path.GetFileName(path);
            if (fileName == null)
                fileName = path.Substring(path.LastIndexOf('/') + 1);
            Title = fileName;
        }

        protected override void Request()
        {
            if (Removed && _parent == null)
            {
                throw new InvalidOperationException("File does not exist!");
            }

            RequestSourceDiff();
        }

        private void RequestSourceDiff()
        {
            var newSource = "";
            var mime = "";
            if (!Removed)
            {
                var file = DownloadFile(_user, _slug, _branch, _path, out mime);
                if (mime.StartsWith("text/plain"))
                    newSource = System.IO.File.ReadAllText(file, System.Text.Encoding.UTF8);
                else
                {
                    LoadFile(file);
                    return;
                }
            }
            
            var oldSource = "";
            if (_parent != null && !Added)
            {
                var file = DownloadFile(_user, _slug, _parent, _path, out mime);
                if (mime.StartsWith("text/plain"))
                    oldSource = System.IO.File.ReadAllText(file, System.Text.Encoding.UTF8);
                else
                {
                    LoadFile(file);
                    return;
                }
            }

            _baseText = JavaScriptStringEncode(oldSource);
            _newText = JavaScriptStringEncode(newSource);
            LoadDiffData();
        }

        protected override bool ShouldStartLoad(NSUrlRequest request, UIWebViewNavigationType navigationType)
        {
            var url = request.Url;
            if(url.Scheme.Equals("app")) {
                var func = url.Host;
                if(func.Equals("comment")) {
                    //var r = new RestSharp.Deserializers.JsonDeserializer().Deserialize<CreateChangesetCommentModel>(new RestSharp.RestResponse { Content = Decode(url.Fragment) });
                    //PromptForComment(r);
                    return false;
                }
            }

            return base.ShouldStartLoad(request, navigationType);
        }

        private void PromptForComment(CreateCommentModel model)
        {
            string title = string.Empty;
            if (model.Position != null)
                title = "Line ".t() + model.Position;

            var sheet = MonoTouch.Utilities.GetSheet(title);
            var addButton = sheet.AddButton("Add Comment".t());
            var cancelButton = sheet.AddButton("Cancel".t());
            sheet.CancelButtonIndex = cancelButton;
            sheet.DismissWithClickedButtonIndex(cancelButton, true);
            sheet.Clicked += (sender, e) => {
                if (e.ButtonIndex == addButton)
                    ShowCommentComposer(model);
            };

            sheet.ShowInView(this.View);
        }

        private void ShowCommentComposer(CreateCommentModel model)
        {
            var composer = new Composer();
            composer.NewComment(this, () => {
                model.Body = composer.Text;
                model.Path = _path;
                composer.DoWork(() => {
//                    var c = Application.Client.Users[_user].Repositories[_slug].Changesets[_branch].Comments.Create(model.Content, model.LineFrom, model.LineTo, model.ParentId, model.Filename);
//
//                    //This will inheriently add it to the controller's comments which we're referencing
//                    if (Comments != null)
//                        Comments.Add(c);
//
//                    var a = new List<ChangesetCommentModel>();
//                    a.Add(c);
//                    AddComments(a);

                    InvokeOnMainThread(() => composer.CloseComposer());
                }, ex => {
                    MonoTouch.Utilities.ShowAlert("Unable to Comment".t(), ex.Message);
                    composer.EnableSendButton = true;
                });
            });
        }

        protected override void DOMReady()
        {
            InvokeOnMainThread(() => Web.EvaluateJavascript("var a = \"" + _baseText + "\"; var b = \"" + _newText + "\"; diff(a, b);"));
            _baseText = _newText = null;
            AddComments(Comments);
        }

        private void AddComments(List<CommentModel> comments)
        {
//            //Convert it to something light weight
            var slimComments = comments.Where(x => string.Equals(x.Path, _path)).Select(x => new { 
                Id = x.Id, User = x.User.Login, Avatar = x.User.AvatarUrl, LineTo = x.Line, LineFrom = x.Line,
                Content = x.Body, Date = x.UpdatedAt
            }).ToList();

            var c = new RestSharp.Serializers.JsonSerializer().Serialize(slimComments);
            BeginInvokeOnMainThread(() => Web.EvaluateJavascript("var a = " + c + "; addComments(a);"));
        }
    }
}

