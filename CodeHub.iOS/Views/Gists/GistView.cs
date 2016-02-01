using System;
using CodeFramework.iOS.ViewControllers;
using CodeFramework.iOS.Views;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.ViewControllers;
using GitHubSharp.Models;
using MonoTouch.Dialog;
using Foundation;
using UIKit;
using CodeFramework.iOS.Utils;

namespace CodeHub.iOS.Views.Gists
{
    public class GistView : ViewModelDrivenDialogViewController
    {
        private readonly UIBarButtonItem _shareButton, _userButton;
        private readonly UIButton _starButton;

        public new GistViewModel ViewModel
        {
            get { return (GistViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public GistView()
        {
            Title = "Gist";

            ToolbarItems = new []
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem((_starButton = ToolbarButton.Create(Images.Gist.Star, () => ViewModel.ToggleStarCommand.Execute(null)))),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                (_userButton = new UIBarButtonItem(ToolbarButton.Create(Images.Gist.User, () => ViewModel.GoToUserCommand.Execute(null)))),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                (_shareButton = new UIBarButtonItem(ToolbarButton.Create(Images.Gist.Share, ShareButtonPress))),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            //Disable these buttons until the gist object becomes valid
            _userButton.Enabled = false;
            _shareButton.Enabled = false;

            ViewModel.Bind(x => x.Gist, gist =>
            {
                UpdateOwned();
                RenderGist();
            });

			ViewModel.BindCollection(x => x.Comments, x => RenderGist());

            ViewModel.Bind(x => x.IsStarred, isStarred =>
            {
                _starButton.SetImage(isStarred ? Images.Gist.StarHighlighted : Images.Gist.Star, UIControlState.Normal);
                _starButton.SetNeedsDisplay();
            });
        }

        private void UpdateOwned()
        {
            //Is it owned?
			var app = Cirrious.CrossCore.Mvx.Resolve<CodeHub.Core.Services.IApplicationService>();
            if (string.Equals(app.Account.Username, ViewModel.Gist.Owner.Login, StringComparison.OrdinalIgnoreCase))
            {
				NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Compose, async (s, e) => {
					try
					{
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
                });
            }
            else
            {
				NavigationItem.RightBarButtonItem = new UIBarButtonItem(Theme.CurrentTheme.ForkButton, UIBarButtonItemStyle.Plain, async (s, e) => {
					try
					{
						NavigationItem.RightBarButtonItem.Enabled = false;
						await this.DoWorkAsync("Forking...", ViewModel.ForkGist);
					}
					catch (Exception ex)
					{
						MonoTouch.Utilities.ShowAlert("Error", ex.Message);
					}
					finally
					{
                        NavigationItem.RightBarButtonItem.Enabled = true;
					}
                });
            }
        }
        
        private void ShareButtonPress()
        {
            if (ViewModel.Gist == null)
                return;

            var sheet = new UIActionSheet();
            var shareButton = sheet.AddButton("Share".t());
            var showButton = sheet.AddButton("Show in GitHub".t());
            var cancelButton = sheet.AddButton("Cancel".t());
            sheet.CancelButtonIndex = cancelButton;
            sheet.DismissWithClickedButtonIndex(cancelButton, true);
            sheet.Dismissed += (s, e) =>
            {
                BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        if (e.ButtonIndex == shareButton)
                        {
                            ViewModel.ShareCommand.Execute(null);
                        }
                        else if (e.ButtonIndex == showButton)
                        {
                            ViewModel.GoToHtmlUrlCommand.Execute(null);
                        }
                    }
                    catch
                    {
                    }
                });

                sheet.Dispose();
            };

            sheet.ShowFrom(_shareButton, true);
        }

        public void RenderGist()
        {
			if (ViewModel.Gist == null)
				return;

			var model = ViewModel.Gist;

            _shareButton.Enabled = _userButton.Enabled = model != null;
            var root = new RootElement(Title) { UnevenRows = true };
			var sec = new Section();
			root.Add(sec);


            var str = string.IsNullOrEmpty(model.Description) ? "Gist " + model.Id : model.Description;
            var d = new NameTimeStringElement() { 
                Time = model.UpdatedAt.ToDaysAgo(), 
                String = str, 
                Image = Images.Avatar
            };

            //Sometimes there's no user!
            d.Name = (model.Owner == null) ? "Anonymous" : model.Owner.Login;
            d.ImageUri = (model.Owner == null) ? null : new Uri(model.Owner.AvatarUrl);

            sec.Add(d);

			var sec2 = new Section("Files");
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
				foreach (var comment in ViewModel.Comments)
				{
					var el = new NameTimeStringElement 
					{ 
						Name = "Anonymous",
                        Image = Images.Avatar,
						String = comment.Body,
			         	Time = comment.CreatedAt.ToDaysAgo(),
					};

					if (comment.User != null)
					{
						el.Name = comment.User.Login;
						el.ImageUri = new Uri(comment.User.AvatarUrl);
					}

					sec3.Add(el);
				}
				root.Add(sec3);
			}

            Root = root;
        }

        public override void ViewWillAppear(bool animated)
        {
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(false, animated);
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            if (ToolbarItems != null)
                NavigationController.SetToolbarHidden(true, animated);
        }
    }
}

