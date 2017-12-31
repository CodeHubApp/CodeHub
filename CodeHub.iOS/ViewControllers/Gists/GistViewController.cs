using System;
using CodeHub.Core.ViewModels.Gists;
using UIKit;
using CodeHub.iOS.Utilities;
using System.Linq;
using System.Threading.Tasks;
using CodeHub.iOS.DialogElements;
using CodeHub.Core.Services;
using MvvmCross.Platform;
using System.Collections.Generic;
using CodeHub.iOS.Services;
using System.Reactive.Linq;
using ReactiveUI;
using Octokit;
using Humanizer;

namespace CodeHub.iOS.ViewControllers.Gists
{
    public class GistViewController : PrettyDialogViewController
    {
        private SplitViewElement _splitRow1, _splitRow2;
        private StringElement _ownerElement;
        private SplitButtonElement _split;
        private readonly IAlertDialogService _alertDialogService = Mvx.Resolve<IAlertDialogService>();

        public new GistViewModel ViewModel
        {
            get { return (GistViewModel)base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public static GistViewController FromGist(Gist gist)
        {
            return new GistViewController
            {
                ViewModel = GistViewModel.FromGist(gist)
            };
        }

        public GistViewController()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "Gist";

            var editButton = NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action);

            HeaderView.SetImage(null, Images.Avatar);
            HeaderView.Text = "Gist #" + ViewModel.Id;
            HeaderView.SubImageView.TintColor = UIColor.FromRGB(243, 156, 18);

            Appeared.Take(1)
                .Select(_ => Observable.Timer(TimeSpan.FromSeconds(0.35f)).Take(1))
                .Switch()
                .Select(_ => ViewModel.Bind(x => x.IsStarred, true))
                .Switch()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => HeaderView.SetSubImage(x ? Octicon.Star.ToImage() : null));

            TableView.RowHeight = UITableView.AutomaticDimension;
            TableView.EstimatedRowHeight = 44f;

            _split = new SplitButtonElement();
            var files = _split.AddButton("Files", "-");
            var comments = _split.AddButton("Comments", "-");
            var forks = _split.AddButton("Forks", "-");

            _splitRow1 = new SplitViewElement(Octicon.Lock.ToImage(), Octicon.Package.ToImage());
            _splitRow2 = new SplitViewElement(Octicon.Calendar.ToImage(), Octicon.Star.ToImage());
            _ownerElement = new StringElement("Owner", string.Empty, UITableViewCellStyle.Value1) { 
                Image = Octicon.Person.ToImage(),
                Accessory = UITableViewCellAccessory.DisclosureIndicator
            };

            OnActivation(d =>
            {
                d(editButton.GetClickedObservable().Subscribe(ShareButtonTap));
                d(_ownerElement.Clicked.BindCommand(ViewModel.GoToUserCommand));

                d(ViewModel.Bind(x => x.IsStarred, true).Subscribe(isStarred => _splitRow2.Button2.Text = isStarred ? "Starred" : "Not Starred"));

                d(ViewModel.BindCollection(x => x.Comments, true).Subscribe(_ => RenderGist()));
                d(HeaderView.Clicked.BindCommand(ViewModel.GoToUserCommand));

                d(ViewModel.Bind(x => x.Gist, true).Where(x => x != null).Subscribe(gist =>
                {
                    var daysOld = gist.CreatedAt.TotalDaysAgo();

                    _splitRow1.Button1.Text = gist.Public ? "Public" : "Private";
                    _splitRow1.Button2.Text = (gist.History?.Count ?? 0) + " Revisions";
                    _splitRow2.Button1.Text = daysOld == 0 ? "Created today" : "day".ToQuantity(daysOld) + " old";
                    _ownerElement.Value = gist.Owner?.Login ?? "Unknown";
                    files.Text = gist.Files?.Count.ToString() ?? "-";
                    comments.Text = gist.Comments.ToString();
                    forks.Text = gist.Forks?.Count.ToString() ?? "-";
                    HeaderView.SubText = gist.Description;
                    HeaderView.Text = gist.Files?.Select(x => x.Key).FirstOrDefault() ?? HeaderView.Text;
                    HeaderView.SetImage(gist.Owner?.AvatarUrl, Images.Avatar);
                    RenderGist();
                    RefreshHeaderView();
                }));
            });
        }

        public void RenderGist()
        {
            if (ViewModel.Gist == null) return;
            var model = ViewModel.Gist;

            ICollection<Section> sections = new LinkedList<Section>();
            sections.Add(new Section { _split });
            sections.Add(new Section { _splitRow1, _splitRow2, _ownerElement });
            var sec2 = new Section();
            sections.Add(sec2);

            var weakVm = new WeakReference<GistViewModel>(ViewModel);
            foreach (var file in model.Files.Keys)
            {
                var sse = new ButtonElement(file, Octicon.FileCode.ToImage())
                { 
                    LineBreakMode = UILineBreakMode.TailTruncation,
                };

                var fileSaved = file;
                var gistFileModel = model.Files[fileSaved];
                sse.Clicked.Subscribe(_ => GoToGist(gistFileModel));
                sec2.Add(sse);
            }

            if (ViewModel.Comments.Items.Count > 0)
            {
                var sec3 = new Section("Comments");
                sec3.AddAll(ViewModel.Comments.Select(x => new CommentElement(x.User?.Login ?? "Anonymous", x.Body, x.CreatedAt, x.User?.AvatarUrl)));
                sections.Add(sec3);
            }

            Root.Reset(sections);
        }

        private void GoToGist(GistFile model)
        {
            var viewCtrl = new GistFileViewController(
                ViewModel.Gist.Id,
                model.Filename,
                ViewModel.Gist);

            this.PushViewController(viewCtrl);
        }

        private async Task Fork()
        {
            try
            {
                await this.DoWorkAsync("Forking...", ViewModel.ForkGist);
            }
            catch (Exception ex)
            {
                _alertDialogService.Alert("Error", ex.Message).ToBackground();
            }
        }

        private async Task Compose()
        {
            try
            {
                var app = Mvx.Resolve<IApplicationService>();
                var data = await this.DoWorkAsync("Loading...", () => app.GitHubClient.Gist.Get(ViewModel.Id));
                var gistController = new GistEditViewController(data);
                gistController.Created = editedGist => ViewModel.Gist = editedGist;
                var navController = new UINavigationController(gistController);
                PresentViewController(navController, true, null);
            }
            catch (Exception ex)
            {
                _alertDialogService.Alert("Error", ex.Message).ToBackground();
            }
        }

        void ShareButtonTap (object sender)
        {
            if (ViewModel.Gist == null)
                return;

            var app = Mvx.Resolve<IApplicationService>();
            var isOwner = string.Equals(app.Account.Username, ViewModel.Gist?.Owner?.Login, StringComparison.OrdinalIgnoreCase);
            var gist = ViewModel.Gist;

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
                        {
                            AlertDialogService.Share(
                                $"Gist {gist.Files?.Select(x => x.Key).FirstOrDefault() ?? gist.Id}",
                                gist.Description,
                                gist.HtmlUrl,
                                sender as UIBarButtonItem);
                        }
                        else if (e.ButtonIndex == showButton)
                            ViewModel.GoToHtmlUrlCommand.Execute(null);
                        else if (e.ButtonIndex == starButton)
                            ViewModel.ToggleStarCommand.Execute(null);
                        else if (e.ButtonIndex == editButton)
                            Compose().ToBackground();
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

