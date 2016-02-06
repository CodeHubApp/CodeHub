using System;
using CodeHub.iOS.ViewControllers;
using CodeHub.iOS.Views;
using CodeHub.Core.ViewModels.Gists;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using UIKit;
using CodeFramework.iOS.Utils;
using System.Linq;
using CodeHub.iOS.DialogElements;
using System.Threading.Tasks;
using CodeHub.iOS.Elements;

namespace CodeHub.iOS.Views.Gists
{
    public class GistView : PrettyDialogViewController
    {
        private SplitViewElement _splitRow1, _splitRow2;
        private StyledStringElement _ownerElement;
        private SplitButtonElement _split;
        private UIBarButtonItem _editButton;

        public new GistViewModel ViewModel
        {
            get { return (GistViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "Gist";

            _editButton = new UIBarButtonItem(UIBarButtonSystemItem.Action, ShareButtonTap);

            HeaderView.SetImage(null, Images.Avatar);
            HeaderView.Text = "Gist #" + ViewModel.Id;
            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 44f;

            _split = new SplitButtonElement();
            var files = _split.AddButton("Files", "-");
            var comments = _split.AddButton("Comments", "-");
            var forks = _split.AddButton("Forks", "-");

            _splitRow1 = new SplitViewElement(Octicon.Lock.ToImage(), Octicon.Package.ToImage());
            _splitRow2 = new SplitViewElement(Octicon.Calendar.ToImage(), Octicon.Star.ToImage());
            _ownerElement = new StyledStringElement("Owner", string.Empty, UITableViewCellStyle.Value1) { 
                Image = Octicon.Person.ToImage(),
                Accessory = UITableViewCellAccessory.DisclosureIndicator
            };
            _ownerElement.Tapped += () => ViewModel.GoToUserCommand.Execute(null);

            ViewModel.Bind(x => x.Gist, gist =>
            {
                _splitRow1.Button1.Text = (gist.Public ?? true) ? "Public" : "Private";
                _splitRow1.Button2.Text = (gist.History?.Count ?? 0) + " Revisions";
                _splitRow2.Button1.Text = gist.CreatedAt.Day + " Days Old";
                _ownerElement.Value = gist.Owner?.Login ?? "Unknown";
                files.Text = gist.Files.Count.ToString();
                comments.Text = gist.Comments.ToString();
                forks.Text = gist.Forks.Count.ToString();
                HeaderView.SubText = gist.Description;
                HeaderView.Text = gist.Files?.Select(x => x.Key).FirstOrDefault() ?? HeaderView.Text;
                HeaderView.SetImage(gist.Owner?.AvatarUrl, Images.Avatar);
                RenderGist();
                RefreshHeaderView();
            });

			ViewModel.BindCollection(x => x.Comments, x => RenderGist());

            ViewModel.Bind(x => x.IsStarred, isStarred =>
            {
                _splitRow2.Button2.Text = isStarred ? "Starred" : "Not Starred";
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationItem.RightBarButtonItem = _editButton;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            NavigationItem.RightBarButtonItem = null;
        }

        public void RenderGist()
        {
			if (ViewModel.Gist == null) return;
			var model = ViewModel.Gist;
            var root = new RootElement(Title);
            root.Add(new Section { _split });
            root.Add(new Section { _splitRow1, _splitRow2, _ownerElement });
			var sec2 = new Section();
			root.Add(sec2);

            foreach (var file in model.Files.Keys)
            {
                var sse = new StyledStringElement(file, model.Files[file].Size + " bytes", UITableViewCellStyle.Subtitle) { 
                    Accessory = UITableViewCellAccessory.DisclosureIndicator, 
                    LineBreakMode = UILineBreakMode.TailTruncation,
                    Lines = 1 
                };

                var fileSaved = file;
                var gistFileModel = model.Files[fileSaved];
                
//				if (string.Equals(gistFileModel.Language, "markdown", StringComparison.OrdinalIgnoreCase))
//					sse.Tapped += () => ViewModel.GoToViewableFileCommand.Execute(gistFileModel);
//				else
					sse.Tapped += () => ViewModel.GoToFileSourceCommand.Execute(gistFileModel);
                sec2.Add(sse);
            }

			if (ViewModel.Comments.Items.Count > 0)
			{
				var sec3 = new Section("Comments");
                sec3.AddAll(ViewModel.Comments.Select(x => new CommentElement(x.User?.Login ?? "Anonymous", x.Body, x.CreatedAt, x.User?.AvatarUrl)));
				root.Add(sec3);
			}

            Root = root;
        }

        private async Task Fork()
        {
            try
            {
                await this.DoWorkAsync("Forking...", ViewModel.ForkGist);
            }
            catch (Exception ex)
            {
                MonoTouch.Utilities.ShowAlert("Error", ex.Message);
            }
        }

        private async Task Compose()
        {
            try
            {
                var app = Cirrious.CrossCore.Mvx.Resolve<CodeHub.Core.Services.IApplicationService>();
                var data = await this.DoWorkAsync("Loading...", () => app.Client.ExecuteAsync(app.Client.Gists[ViewModel.Id].Get()));
                var gistController = new EditGistController(data.Data);
                gistController.Created = editedGist => ViewModel.Gist = editedGist;
                var navController = new UINavigationController(gistController);
                PresentViewController(navController, true, null);
            }
            catch (Exception ex)
            {
                MonoTouch.Utilities.ShowAlert("Error", ex.Message);
            }
        }

        void ShareButtonTap (object sender, EventArgs args)
        {
            if (ViewModel.Gist == null)
                return;

            var app = Cirrious.CrossCore.Mvx.Resolve<CodeHub.Core.Services.IApplicationService>();
            var isOwner = string.Equals(app.Account.Username, ViewModel.Gist.Owner.Login, StringComparison.OrdinalIgnoreCase);

            var sheet = new UIActionSheet();
            var editButton = sheet.AddButton(isOwner ? "Edit" : "Fork");
            var starButton = sheet.AddButton(ViewModel.IsStarred ? "Unstar" : "Star");
            var shareButton = sheet.AddButton("Share");
            var showButton = sheet.AddButton("Show in GitHub");
            var cancelButton = sheet.AddButton("Cancel");
            sheet.CancelButtonIndex = cancelButton;
            sheet.DismissWithClickedButtonIndex(cancelButton, true);
            sheet.Dismissed += (s, e) =>
            {
                BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        if (e.ButtonIndex == shareButton)
                            ViewModel.ShareCommand.Execute(null);
                        else if (e.ButtonIndex == showButton)
                            ViewModel.GoToHtmlUrlCommand.Execute(null);
                        else if (e.ButtonIndex == starButton)
                            ViewModel.ToggleStarCommand.Execute(null);
                        else if (e.ButtonIndex == editButton)
                            Compose();
                    }
                    catch
                    {
                    }
                });

                sheet.Dispose();
            };

            sheet.ShowFromToolbar(NavigationController.Toolbar);
        }
    }
}

