using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CodeHub.Core.ViewModels.User;
using GitHubSharp.Models;
using CodeHub.Core.ViewModels;
using MvvmCross.Core.ViewModels;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistViewModel : LoadableViewModel
    {
        private readonly CollectionViewModel<GistCommentModel> _comments = new CollectionViewModel<GistCommentModel>();
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
            set { this.RaiseAndSetIfChanged(ref _gist, value); }
        }

        public bool IsStarred
        {
            get { return _starred; }
            private set { this.RaiseAndSetIfChanged(ref _starred, value); }
        }

        public CollectionViewModel<GistCommentModel> Comments
        {
            get { return _comments; }
        }

        public ICommand GoToUserCommand
        {
            get { return new MvxCommand(() => ShowViewModel<UserViewModel>(new UserViewModel.NavObject { Username = Gist.Owner.Login }), () => Gist != null && Gist.Owner != null); }
        }

        public ICommand GoToFileSourceCommand
        {
            get { 
                return new MvxCommand<GistFileModel>(x => {
                    GetService<CodeHub.Core.Services.IViewModelTxService>().Add(x);
                    ShowViewModel<GistFileViewModel>(new GistFileViewModel.NavObject { GistId = Id, Filename = x.Filename });
                });
            }
        }

        public ICommand GoToHtmlUrlCommand
        {
            get { return new MvxCommand(() => GoToUrlCommand.Execute(_gist.HtmlUrl), () => _gist != null); }
        }

        public ICommand ForkCommand
        {
            get
            {
                return new MvxCommand(() => ForkGist());
            }
        }

        public ICommand ToggleStarCommand
        {
            get
            {
                return new MvxCommand(() => ToggleStarred(), () => Gist != null);
            }
        }

        public void Init(NavObject navObject)
        {
            Id = navObject.Id;
        }

        private async Task ToggleStarred()
        {
            try
            {
                var request = IsStarred ? this.GetApplication().Client.Gists[Id].Unstar() : this.GetApplication().Client.Gists[Id].Star();
                await this.GetApplication().Client.ExecuteAsync(request);
                IsStarred = !IsStarred;
            }
            catch
            {
                DisplayAlert("Unable to start gist. Please try again.");
            }
        }

        public async Task ForkGist()
        {
            var data = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Gists[Id].ForkGist());
            var forkedGist = data.Data;
            ShowViewModel<GistViewModel>(new GistViewModel.NavObject { Id = forkedGist.Id });
        }

        protected override Task Load()
        {
            var t1 = this.RequestModel(this.GetApplication().Client.Gists[Id].Get(), response => Gist = response.Data);
            this.RequestModel(this.GetApplication().Client.Gists[Id].IsGistStarred(), response => IsStarred = response.Data).FireAndForget();
            Comments.SimpleCollectionLoad(this.GetApplication().Client.Gists[Id].GetComments()).FireAndForget();
            return t1;
        }

        public async Task Edit(GistEditModel editModel)
        {
            var response = await this.GetApplication().Client.ExecuteAsync(this.GetApplication().Client.Gists[Id].EditGist(editModel));
            Gist = response.Data;
        }

        public class NavObject
        {
            public string Id { get; set; }
        }
    }
}

