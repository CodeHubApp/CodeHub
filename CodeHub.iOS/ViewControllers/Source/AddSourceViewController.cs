using System;
using UIKit;
using CodeHub.iOS.Utilities;
using System.Threading.Tasks;
using CodeHub.iOS.Services;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.TableViewSources;
using CodeHub.Core.Services;
using Splat;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace CodeHub.iOS.ViewControllers.Source
{
    public class AddSourceViewController : TableViewController
    {
        private readonly IApplicationService _applicationService;
        private readonly string _username;
        private readonly string _repository;
        private readonly string _path;
        private readonly string _branch;

        private readonly DummyInputElement _titleElement;
        private readonly ExpandingInputElement _descriptionElement;

        private readonly ISubject<Octokit.RepositoryContentChangeSet> _successSubject
            = new Subject<Octokit.RepositoryContentChangeSet>();

        public IObservable<Octokit.RepositoryContentChangeSet> Success => _successSubject.AsObservable();

        public AddSourceViewController(
            string username,
            string repository,
            string path,
            string branch,
            IApplicationService applicationService = null)
            : base(UITableViewStyle.Plain)
        {
            _username = username;
            _repository = repository;
            _path = path;
            _branch = branch;
            _applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();

            _titleElement = new DummyInputElement("Name") { SpellChecking = false };
            _descriptionElement = new ExpandingInputElement("Content")
            {
                SpellChecking = false,
                Font = UIFont.FromName("Courier", UIFont.PreferredBody.PointSize)
            };

            EdgesForExtendedLayout = UIRectEdge.None;
            Title = "Add File";

            var commitButton = new UIBarButtonItem { Title = "Commit" };
            NavigationItem.RightBarButtonItem = commitButton;

            this.OnActivation(d =>
            {
                d(commitButton
                  .GetClickedObservable()
                  .Subscribe(_ => Commit()));

                d(_titleElement
                  .Changed
                  .Select(x => x.Length != 0)
                  .Subscribe(x => commitButton.Enabled = x));
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new DialogTableViewSource(TableView);
            source.Root.Add(new Section { _titleElement, _descriptionElement });
            TableView.Source = source;
            TableView.TableFooterView = new UIView();
        }

        private void Commit()
        {
            var content = _descriptionElement.Value;

            var composer = new Composer
            {
                Title = "Commit Message",
                Text = "Create " + _titleElement.Value
            };

            composer
                .SendItem
                .GetClickedObservable()
                .Subscribe(_ => CommitThis(composer.Text).ToBackground());

            this.PushViewController(composer);
        }

        private async Task CommitThis(string message)
        {
            var content = _descriptionElement.Value;
            var name = _titleElement.Value;
            var path = string.IsNullOrEmpty(_path) ? name : $"{_path.TrimEnd('/')}/{name}";
            var hud = this.CreateHud();

            try
            {
                hud.Show("Committing...");
                UIApplication.SharedApplication.BeginIgnoringInteractionEvents();
                NetworkActivity.PushNetworkActive();

                var result = await _applicationService.GitHubClient.Repository.Content.CreateFile(
                    _username, _repository, path, new Octokit.CreateFileRequest(message, content, _branch));

                this.PresentingViewController?.DismissViewController(true, null);

                _successSubject.OnNext(result);
            }
            catch (Octokit.ApiException ex)
            {
                var errorMessage = ex.Message;
                if (ex.ApiError?.DocumentationUrl == "https://developer.github.com/v3/repos/contents/#update-a-file")
                    errorMessage = "A file with that name already exists!";

                AlertDialogService.ShowAlert("Error", errorMessage);
            }
            catch (Exception ex)
            {
                AlertDialogService.ShowAlert("Error", ex.Message);
            }
            finally
            {
                UIApplication.SharedApplication.EndIgnoringInteractionEvents();
                NetworkActivity.PopNetworkActive();
                hud.Hide();
            }
        }
    }
}

