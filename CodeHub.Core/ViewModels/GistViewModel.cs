using CodeFramework.Core.ViewModels;
using GitHubSharp.Models;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels
{
    public class GistViewModel : BaseViewModel, ILoadableViewModel
    {
        private GistModel _gist;
        private bool _starred;

        public string Id
        {
            get;
            private set;
        }

        public GistModel Gist
        {
            get { return _gist; }
            set
            {
                _gist = value;
                RaisePropertyChanged(() => Gist);
            }
        }

        public bool IsStarred
        {
            get { return _starred; }
            private set
            {
                _starred = value;
                RaisePropertyChanged(() => IsStarred);
            }
        }

        public void Init(NavObject navObject)
        {
            Id = navObject.Id;
        }

        public async Task SetStarred(bool value)
        {
            var request = value ? Application.Client.Gists[Id].Star() : Application.Client.Gists[Id].Unstar();
            await Application.Client.ExecuteAsync(request);
            IsStarred = value;
        }

        public Task Load(bool forceDataRefresh)
        {
            var t1 = Task.Run(() => this.RequestModel(Application.Client.Gists[Id].Get(), forceDataRefresh, response => {
                Gist = response.Data;
            }));

            FireAndForgetTask.Start(() => this.RequestModel(Application.Client.Gists[Id].IsGistStarred(), forceDataRefresh, response => IsStarred = response.Data));

            return t1;
        }

        public async Task Edit(GistEditModel editModel)
        {
            var response = await Application.Client.ExecuteAsync(Application.Client.Gists[Id].EditGist(editModel));
            Gist = response.Data;
        }

        public class NavObject
        {
            public string Id { get; set; }
        }
    }
}

