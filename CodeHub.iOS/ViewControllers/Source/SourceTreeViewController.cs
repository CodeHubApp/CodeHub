using System;
using System.Reactive.Linq;
using CodeHub.Core;
using CodeHub.Core.Services;
using CodeHub.iOS.DialogElements;
using CodeHub.iOS.Views;
using ReactiveUI;
using Splat;
using UIKit;
using System.Linq;
using System.Collections.Generic;
using CodeHub.Core.Utils;
using CodeHub.iOS.Utilities;

namespace CodeHub.iOS.ViewControllers.Source
{
    public class SourceTreeViewController : DialogViewController
    {
        private readonly SourceTitleView _titleView = new SourceTitleView();
        private bool _branchSelectorShowsBranches = true;
        private readonly string _username;
        private readonly string _repository;
        private readonly string _path;
        private string _sha;
        private ShaType _shaType;

        public SourceTreeViewController(
            string username,
            string repository,
            string path,
            string sha,
            ShaType shaType,
            IApplicationService applicationService = null,
            IFeaturesService featuresService = null)
            : base(style: UITableViewStyle.Plain)
        {
            _username = username;
            _repository = repository;
            _path = path;
            _sha = sha;
            _shaType = shaType;

            applicationService = applicationService ?? Locator.Current.GetService<IApplicationService>();
            featuresService = featuresService ?? Locator.Current.GetService<IFeaturesService>();

            var loadContents = ReactiveCommand.CreateFromTask((string shaRef) =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    return applicationService
                        .GitHubClient.Repository.Content
                        .GetAllContentsByRef(username, repository, shaRef);
                }

                return applicationService
                    .GitHubClient.Repository.Content
                    .GetAllContentsByRef(username, repository, path, shaRef);
            });

            loadContents
                .ThrownExceptions
                .Select(HandleLoadError)
                .SelectMany(Interactions.Errors.Handle)
                .Subscribe();

            OnActivation(d =>
            {
                d(_titleView
                  .GetClickedObservable()
                  .Subscribe(_ => ShowBranchSelector()));
            });

            Appearing
                .Select(_ => _sha)
                .Where(x => !string.IsNullOrEmpty(x))
                .DistinctUntilChanged()
                .InvokeReactiveCommand(loadContents);

            loadContents
                .Subscribe(SetElements);

            NavigationItem.TitleView = _titleView;
        }

        private UserError HandleLoadError(Exception error)
        {
            Root.Reset();

            var apiException = error as Octokit.ApiException;
            if (apiException?.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new UserError($"The current directory does not exist under the selected Git reference ({_sha})");

            return new UserError("Unable to load source tree: " + error.Message);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            _titleView.SubText = _shaType == ShaType.Hash
                ? _sha.Substring(0, Math.Min(_sha.Length, 7))
                : _sha;

            if (string.IsNullOrEmpty(_path))
                _titleView.Text = _repository;
            else
            {
                var path = _path.TrimEnd('/');
                _titleView.Text = path.Substring(path.LastIndexOf('/') + 1);
            }
        }

        private void SetElements(IEnumerable<Octokit.RepositoryContent> items)
        {
            var elements = items.Select(CreateElement);
            Root.Reset(new Section { elements });
        }

        private void ShowBranchSelector()
        {
            var view = _branchSelectorShowsBranches
                ? BranchesAndTagsViewController.SelectedView.Branches
                : BranchesAndTagsViewController.SelectedView.Tags;

            var viewController = new BranchesAndTagsViewController(_username, _repository, view);

            viewController.TagSelected.Take(1).Subscribe(tag =>
            {
                _sha = tag.Name;
                _shaType = ShaType.Tag;
                _branchSelectorShowsBranches = false;
                this.DismissViewController(true, null);
            });

            viewController.BranchSelected.Take(1).Subscribe(branch =>
            {
                _sha = branch.Name;
                _shaType = ShaType.Branch;
                _branchSelectorShowsBranches = true;
                this.DismissViewController(true, null);
            });

            this.PresentModalViewController(viewController);
        }

        private void GoToSourceTree(Octokit.RepositoryContent content)
        {
            this.PushViewController(new SourceTreeViewController(
                _username, _repository, content.Path, _sha, _shaType));
        }

        private void GoToSubModule(Octokit.RepositoryContent content)
        {
            var gitUrl = content.GitUrl;
            var nameAndSlug = gitUrl.Substring(gitUrl.IndexOf("/repos/", StringComparison.Ordinal) + 7);
            var indexOfGit = nameAndSlug.LastIndexOf("/git", StringComparison.Ordinal);
            indexOfGit = indexOfGit < 0 ? 0 : indexOfGit;
            var repoId = RepositoryIdentifier.FromFullName(nameAndSlug.Substring(0, indexOfGit));
            if (repoId == null)
                return;

            var sha = gitUrl.Substring(gitUrl.LastIndexOf("/", StringComparison.Ordinal) + 1);

            this.PushViewController(new SourceTreeViewController(
                repoId.Owner, repoId.Name, null, sha, ShaType.Hash));
        }

        private void GoToFile(Octokit.RepositoryContent content)
        {
            var viewController = new FileSourceViewController(
                _username, _repository, content.Path, _sha, _shaType)
            {
                Content = content
            };

            this.PushViewController(viewController);
        }

        private Element CreateElement(Octokit.RepositoryContent content)
        {
            var weakRef = new WeakReference<SourceTreeViewController>(this);

            if (content.Type == Octokit.ContentType.Dir)
            {
                var e = new StringElement(content.Name, Octicon.FileDirectory.ToImage());
                e.Clicked.Subscribe(_ => weakRef.Get()?.GoToSourceTree(content));
                return e;
            }

            if (content.Type == Octokit.ContentType.File)
            {
                if (content.DownloadUrl != null)
                {
                    var e = new StringElement(content.Name, Octicon.FileCode.ToImage());
                    e.Clicked.Subscribe(_ => weakRef.Get()?.GoToFile(content));
                    return e;
                }
                else
                {
                    var e = new StringElement(content.Name, Octicon.FileSubmodule.ToImage());
                    e.Clicked.Subscribe(_ => weakRef.Get()?.GoToSubModule(content));
                    return e;
                }
            }

            return new StringElement(content.Name) { Image = Octicon.FileMedia.ToImage() };
        }
    }
}
