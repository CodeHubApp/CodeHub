using System;
using System.Reactive.Linq;
using CodeHub.Core.Services;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels.Source;
using ReactiveUI;
using Xamarin.Utilities.Core.ReactiveAddons;
using Xamarin.Utilities.Core.ViewModels;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestFilesViewModel : LoadableViewModel
    {
        public ReactiveCollection<CommitModel.CommitFileModel> Files { get; private set; }

        public long PullRequestId { get; set; }

        public string Username { get; set; }

        public string Repository { get; set; }

		public IReactiveCommand GoToSourceCommand { get; private set; }

        public PullRequestFilesViewModel(IApplicationService applicationService)
        {
            Files = new ReactiveCollection<CommitModel.CommitFileModel>
            {
                GroupFunc = y =>
                {
                    var filename = "/" + y.Filename;
                    return filename.Substring(0, filename.LastIndexOf("/", StringComparison.Ordinal) + 1);
                }
            };

            GoToSourceCommand =  new ReactiveCommand();
            GoToSourceCommand.OfType<CommitModel.CommitFileModel>().Subscribe(x =>
            {
                var vm = CreateViewModel<SourceViewModel>();
                vm.Name = x.Filename.Substring(x.Filename.LastIndexOf("/", StringComparison.Ordinal) + 1);
                vm.Path = x.Filename;
                vm.GitUrl = x.ContentsUrl;
                vm.ForceBinary = x.Patch == null;
                ShowViewModel(vm);
            });

            LoadCommand.RegisterAsyncTask(t =>
                Files.SimpleCollectionLoad(
                    applicationService.Client.Users[Username].Repositories[Repository].PullRequests[PullRequestId]
                        .GetFiles(), t as bool?));
        }
    }
}

