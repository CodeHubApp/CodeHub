using System;
using CodeFramework.ViewModels;
using CodeFramework.Utils;
using GitHubSharp.Models;
using System.Threading.Tasks;
using GitHubSharp;
using System.Collections.Generic;

namespace CodeHub.ViewModels
{
    public class ChangesetViewModel : ViewModelBase
    {
        private CustomObservableCollection<CommitModel> _items;
        private Task _more;

        public string Username
        {
            get;
            private set;
        }

        public string Repository
        {
            get;
            private set;
        }
        
        public CustomObservableCollection<CommitModel> Items
        {
            get { return _items; }
        }

        public Task More
        {
            get { return _more; }
            private set { SetProperty(ref _more, value); }
        }

        public ChangesetViewModel(string username, string repository)
        {
            Username = username;
            Repository = repository;
            _items = new CustomObservableCollection<CommitModel>();
        }

        public override async Task Load(bool forceDataRefresh)
        {
            await Task.Run(() => this.RequestModel(GetRequest(), forceDataRefresh, response => {
                Items.Reset(response.Data);
                this.CreateMore(response, m => More = m, d => Items.AddRange(d));
            }));
        }

        protected virtual GitHubRequest<List<CommitModel>> GetRequest()
        {
            return Application.Client.Users[Username].Repositories[Repository].Commits.GetAll();
        }
    }
}

