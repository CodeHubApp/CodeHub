using System;
using System.Reactive.Linq;
using CodeFramework.iOS.ViewComponents;
using CodeFramework.iOS.Views;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Gists;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.Core.Services;

namespace CodeHub.iOS.Views.Gists
{
    public class GistView : ViewModelDialogView<GistViewModel>
    {
        private readonly IStatusIndicatorService _statusIndicatorService;
        private readonly IApplicationService _applicationService;
        private UIBarButtonItem _shareButton, _userButton;
        private UIButton _starButton;
        private UIActionSheet _actionSheet;

        public GistView(IStatusIndicatorService statusIndicatorService, IApplicationService applicationService)
        {
            _statusIndicatorService = statusIndicatorService;
            _applicationService = applicationService;
        }

        public override void ViewDidLoad()
        {
            Title = "Gist";

            base.ViewDidLoad();

            ToolbarItems = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem((_starButton = ToolbarButton.Create(Images.Gist.Star, () => ViewModel.ToggleStarCommand.Execute(null)))),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                (_userButton = new UIBarButtonItem(ToolbarButton.Create(Images.Gist.User, () => ViewModel.GoToUserCommand.Execute(null)))),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                (_shareButton = new UIBarButtonItem(ToolbarButton.Create(Images.Gist.Share, ShareButtonPress))),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

            _shareButton.EnableIfExecutable(ViewModel.WhenAnyValue(x => x.Gist, x => x != null));

            //Disable these buttons until the gist object becomes valid
            _userButton.Enabled = false;
            _shareButton.Enabled = false;

            ViewModel.WhenAnyValue(x => x.Gist).Where(x => x != null).Subscribe(gist =>
            {
                if (string.Equals(_applicationService.Account.Username, ViewModel.Gist.Owner.Login, StringComparison.OrdinalIgnoreCase))
                {
                    NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Compose, (s, e) => { });

//                    			try
//					{
//						var data = await this.DoWorkAsync("Loading...", () => app.Client.ExecuteAsync(app.Client.Gists[ViewModel.Id].Get()));
//						var gistController = new EditGistController(data.Data);
//						gistController.Created = editedGist => ViewModel.Gist = editedGist;
//						var navController = new UINavigationController(gistController);
//						PresentViewController(navController, true, null);
//
//					}
//					catch (Exception ex)
//					{
//						MonoTouch.Utilities.ShowAlert("Error", ex.Message);
//					}
                }
                else
                {
                    NavigationItem.RightBarButtonItem = 
                        new UIBarButtonItem(Theme.CurrentTheme.ForkButton, UIBarButtonItemStyle.Plain, (s, e) => 
                            ViewModel.ForkCommand.ExecuteIfCan());
                    NavigationItem.RightBarButtonItem.EnableIfExecutable(ViewModel.ForkCommand.CanExecuteObservable);
                    
                }

                RenderGist();
            });

			ViewModel.Comments.Changed.Subscribe(x => RenderGist());

            ViewModel.WhenAnyValue(x => x.IsStarred).Subscribe(isStarred =>
            {
                _starButton.Enabled = isStarred.HasValue;
                _starButton.SetImage((isStarred.HasValue && isStarred.Value) ? Images.Gist.StarHighlighted : Images.Gist.Star, UIControlState.Normal);
                _starButton.SetNeedsDisplay();
            });

            ViewModel.ForkCommand.IsExecuting.Subscribe(x =>
            {
                if (x)
                    _statusIndicatorService.Show("Forking...");
                else
                    _statusIndicatorService.Hide();
            });
        }
        
        private void ShareButtonPress()
        {
            _actionSheet = new UIActionSheet("Gist");
            var shareButton = _actionSheet.AddButton("Share");
            var showButton = _actionSheet.AddButton("Show in GitHub");
            var cancelButton = _actionSheet.AddButton("Cancel");
            _actionSheet.CancelButtonIndex = cancelButton;
            _actionSheet.DismissWithClickedButtonIndex(cancelButton, true);
            _actionSheet.Clicked += (s, e) => 
			{
	            if (e.ButtonIndex == shareButton)
					ViewModel.ShareCommand.Execute(null);
	            else if (e.ButtonIndex == showButton)
					ViewModel.GoToHtmlUrlCommand.Execute(null);
			    _actionSheet = null;
			};

            _actionSheet.ShowFrom(_shareButton, true);
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
                Image = Theme.CurrentTheme.AnonymousUserImage
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

			if (ViewModel.Comments.Count > 0)
			{
				var sec3 = new Section("Comments");
				foreach (var comment in ViewModel.Comments)
				{
					var el = new NameTimeStringElement 
					{ 
						Name = "Anonymous",
						Image = Theme.CurrentTheme.AnonymousUserImage,
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

