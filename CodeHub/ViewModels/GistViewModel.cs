using System;
using GitHubSharp.Models;
using System.Threading.Tasks;
using CodeFramework.ViewModels;

namespace CodeHub.ViewModels
{
    public class GistViewModel : ViewModelBase
    {
        private readonly string _id;
        private GistModel _gist;
        private bool _starred;

        public GistModel Gist
        {
            get { return _gist; }
            private set { SetProperty(ref _gist, value); }
        }

        public bool IsStarred
        {
            get { return _starred; }
            private set { SetProperty(ref _starred, value); }
        }

        public GistViewModel(string id)
        {
            _id = id;
        }

        public async Task SetStarred(bool value)
        {
            var request = value ? Application.Client.Gists[_id].Star() : Application.Client.Gists[_id].Unstar();
            await Application.Client.ExecuteAsync(request);
            IsStarred = value;
        }

        public override async Task Load(bool forceDataRefresh)
        {
            var t1 = Task.Run(() => this.RequestModel(Application.Client.Gists[_id].Get(), forceDataRefresh, response => {
                Gist = response.Data;
            }));

            new Task(() => {
                try
                {
                    this.RequestModel(Application.Client.Gists[_id].IsGistStarred(), forceDataRefresh, response => {
                        IsStarred = response.Data;
                    });
                }
                catch (Exception e)
                {

                }
            }).Start();

            await t1;
        }
    }
}

