using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.Source;
using ReactiveUI;

using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestFilesViewModel : BaseViewModel, ILoadableViewModel
    {
        public IReadOnlyReactiveList<CommitModel.CommitFileModel> Files { get; private set; }

        public long PullRequestId { get; set; }

        public string Username { get; set; }

        public string Repository { get; set; }

        public IReactiveCommand<object> GoToSourceCommand { get; private set; }

        public IReactiveCommand LoadCommand { get; private set; }

        public PullRequestFilesViewModel(IApplicationService applicationService)
        {
            var files = new ReactiveList<CommitModel.CommitFileModel>()
            {
//                GroupFunc = y =>
//                {
//                    var filename = "/" + y.Filename;
//                    return filename.Substring(0, filename.LastIndexOf("/", StringComparison.Ordinal) + 1);
//                }
            };
            Files = files.CreateDerivedCollection(x => x);

            GoToSourceCommand =  ReactiveCommand.Create();
            GoToSourceCommand.OfType<CommitModel.CommitFileModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<SourceViewModel>();
//                vm.Name = x.Filename.Substring(x.Filename.LastIndexOf("/", StringComparison.Ordinal) + 1);
//                vm.Path = x.Filename;
//                vm.GitUrl = x.ContentsUrl;
//                vm.ForceBinary = x.Patch == null;
                ShowViewModel(vm);
            });

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
                files.SimpleCollectionLoad(
                    applicationService.Client.Users[Username].Repositories[Repository].PullRequests[PullRequestId]
                        .GetFiles(), t as bool?));
        }
    }
}

