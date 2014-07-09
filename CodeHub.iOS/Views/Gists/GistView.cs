using System;
using System.Reactive.Linq;
using CodeFramework.iOS.Views;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Gists;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using ReactiveUI;
using Xamarin.Utilities.Core.Services;

namespace CodeHub.iOS.Views.Gists
{
    public class GistView : ViewModelPrettyDialogView<GistViewModel>
    {
        private readonly IStatusIndicatorService _statusIndicatorService;
        private readonly IApplicationService _applicationService;
        private UIActionSheet _actionSheet;

        public GistView(IStatusIndicatorService statusIndicatorService, IApplicationService applicationService)
        {
            _statusIndicatorService = statusIndicatorService;
            _applicationService = applicationService;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = string.Format("Gist #{0}", ViewModel.Id);
            var updatedGistObservable = ViewModel.WhenAnyValue(x => x.Gist).Where(x => x != null);

            var root = new RootElement(string.Empty) { UnevenRows = true };
            var headerSection = new Section(HeaderView);
            var detailsSection = new Section();
            var filesSection = new Section("Files");
            var commentsSection = new Section("Comments");

            var split = new SplitButtonElement();
            var files = split.AddButton("Files", "-", () => {});
            var comments = split.AddButton("Comments", "-", () => {});
            var forks = split.AddButton("Forks", "-", () => ViewModel.GoToForksCommand.ExecuteIfCan());
            headerSection.Add(split);

            var descriptionElement = new MultilinedElement(string.Empty);
            detailsSection.Add(descriptionElement);

            root.Add(headerSection, detailsSection, filesSection, commentsSection);
            Root = root;

            updatedGistObservable.SubscribeSafe(x =>
            {
                HeaderView.SubText = string.Format("Created {0}", x.CreatedAt.ToDaysAgo());
                if (x.Owner != null) HeaderView.ImageUri = x.Owner.AvatarUrl;
                ReloadData();
            });

            updatedGistObservable.SubscribeSafe(x => files.Text = x.Files.Count.ToString());
            updatedGistObservable.SubscribeSafe(x => comments.Text = x.Comments.ToString());
            updatedGistObservable.SubscribeSafe(x => forks.Text = x.Forks.Count.ToString());
            updatedGistObservable.SubscribeSafe(x => descriptionElement.Caption = x.Description);
            updatedGistObservable.Take(1).SubscribeSafe(x => detailsSection.Visible = !string.IsNullOrEmpty(x.Description));


//            var splitElement1 = new SplitElement();
//            detailsSection.Add(splitElement1);
//
//            updatedGistObservable.Subscribe(x =>
//            {
//                splitElement1.Button1 = new SplitElement.SplitButton(x.Public.HasValue && x.Public.Value ? Images.Unlocked : Images.Locked, model.Private ? "Private" : "Public");
//                splitElement1.Button2 = new SplitElement.SplitButton(Images.Language, model.Language ?? "N/A");
//            });

//            updatedGistObservable.Take(1).Subscribe(_ => root.Add(detailsSection));
//
//
            updatedGistObservable.Subscribe(x =>
            {
                foreach (var file in x.Files.Keys)
                {
                    var sse = new StyledStringElement(file, x.Files[file].Size + " bytes", UITableViewCellStyle.Subtitle) { 
                        Accessory = UITableViewCellAccessory.DisclosureIndicator, 
                        LineBreakMode = UILineBreakMode.TailTruncation,
                        Lines = 1 
                    };

                    var fileSaved = file;
                    var gistFileModel = x.Files[fileSaved];

                    //              if (string.Equals(gistFileModel.Language, "markdown", StringComparison.OrdinalIgnoreCase))
                    //                  sse.Tapped += () => ViewModel.GoToViewableFileCommand.Execute(gistFileModel);
                    //              else
                    sse.Tapped += () => ViewModel.GoToFileSourceCommand.Execute(gistFileModel);

                    filesSection.Add(sse);
                }

                filesSection.Visible = filesSection.Count > 0;
            });
//
//            updatedGistObservable.Take(1).Subscribe(_ => Root.Add(filesSection));

//
//            ViewModel.Comments.Changed.Subscribe(_ =>
//            {
//                foreach (var comment in ViewModel.Comments)
//                {
//                    var el = new NameTimeStringElement 
//                    { 
//                        Name = "Anonymous",
//                        Image = Theme.CurrentTheme.AnonymousUserImage,
//                        String = comment.Body,
//                        Time = comment.CreatedAt.ToDaysAgo(),
//                    };
//
//                    if (comment.User != null)
//                    {
//                        el.Name = comment.User.Login;
//                        el.ImageUri = new Uri(comment.User.AvatarUrl);
//                    }
//
//                    commentsSection.Add(el);
//                }
//            });
//
//            ViewModel.Comments.Changed.Skip(1).Take(1).Subscribe(_ => Root.Insert(Root.Count, commentsSection));

//
            //Sometimes there's no user!
//            d.Name = (model.Owner == null) ? "Anonymous" : model.Owner.Login;
//            d.ImageUri = (model.Owner == null) ? null : new Uri(model.Owner.AvatarUrl);
//
//            sec.Add(d);
//
//            var sec2 = new Section("Files");
//            root.Add(sec2);
//
//            foreach (var file in model.Files.Keys)
//            {
//                var sse = new StyledStringElement(file, model.Files[file].Size + " bytes", UITableViewCellStyle.Subtitle) { 
//                    Accessory = UITableViewCellAccessory.DisclosureIndicator, 
//                    LineBreakMode = UILineBreakMode.TailTruncation,
//                    Lines = 1 
//                };
//
//                var fileSaved = file;
//                var gistFileModel = model.Files[fileSaved];
//
//                //              if (string.Equals(gistFileModel.Language, "markdown", StringComparison.OrdinalIgnoreCase))
//                //                  sse.Tapped += () => ViewModel.GoToViewableFileCommand.Execute(gistFileModel);
//                //              else
//                sse.Tapped += () => ViewModel.GoToFileSourceCommand.Execute(gistFileModel);
//
//
//                sec2.Add(sse);
//            }
//
//            if (ViewModel.Comments.Count > 0)
//            {
//                var sec3 = new Section("Comments");
//                foreach (var comment in ViewModel.Comments)
//                {
//                    var el = new NameTimeStringElement 
//                    { 
//                        Name = "Anonymous",
//                        Image = Theme.CurrentTheme.AnonymousUserImage,
//                        String = comment.Body,
//                        Time = comment.CreatedAt.ToDaysAgo(),
//                    };
//
//                    if (comment.User != null)
//                    {
//                        el.Name = comment.User.Login;
//                        el.ImageUri = new Uri(comment.User.AvatarUrl);
//                    }
//
//                    sec3.Add(el);
//                }
//                root.Add(sec3);
//            }


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
            });


//            ViewModel.WhenAnyValue(x => x.IsStarred).Subscribe(isStarred =>
//            {
//                _starButton.Enabled = isStarred.HasValue;
//                _starButton.SetImage((isStarred.HasValue && isStarred.Value) ? Images.Gist.StarHighlighted : Images.Gist.Star, UIControlState.Normal);
//                _starButton.SetNeedsDisplay();
//            });
//
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
					ViewModel.ShareCommand.ExecuteIfCan();
	            else if (e.ButtonIndex == showButton)
					ViewModel.GoToHtmlUrlCommand.ExecuteIfCan();
			    _actionSheet = null;
			};

            _actionSheet.ShowInView(View);
        }
    }
}

