using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.MvvmCross.ViewModels;
using CodeHub.Core.ViewModels.User;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistViewModel : LoadableViewModel
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

        public ICommand GoToUserCommand
        {
            get { return new MvxCommand(() => ShowViewModel<ProfileViewModel>(new ProfileViewModel.NavObject { Username = Gist.User.Login }), () => Gist != null && Gist.User != null); }
        }

        public ICommand GoToFileSourceCommand
        {
            get { return new MvxCommand(() => { }); }
        }

        public ICommand GoToViewableFileCommand
        {
            get { return new MvxCommand<GistFileModel>(x => ShowViewModel<GistViewableFileViewModel>(new GistViewableFileViewModel.NavObject { GistFile = x }), x => x != null); }
        }

        public ICommand ToggleStarCommand
        {
            get
            {
                return new MvxCommand(ToggleStarred, () => Gist != null);
            }
        }

        public void Init(NavObject navObject)
        {
            Id = navObject.Id;
        }

        private async void ToggleStarred()
        {
            try
            {
                var request = IsStarred ? Application.Client.Gists[Id].Unstar() : Application.Client.Gists[Id].Star();
                await Application.Client.ExecuteAsync(request);
                IsStarred = !IsStarred;
            }
            catch (Exception)
            {
                // Do nothing
            }
        }

        protected override Task Load(bool forceDataRefresh)
        {
            var t1 = Task.Run(() => this.RequestModel(Application.Client.Gists[Id].Get(), forceDataRefresh, response => Gist = response.Data));
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

