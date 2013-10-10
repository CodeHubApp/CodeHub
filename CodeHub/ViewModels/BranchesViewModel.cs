using System;
using CodeFramework.ViewModels;
using GitHubSharp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using CodeFramework.Utils;

namespace CodeHub.ViewModels
{
    public class BranchesViewModel : ViewModelBase
    {
        private CustomObservableCollection<BranchModel> _items;
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

        public CustomObservableCollection<BranchModel> Items
        {
            get { return _items; }
        }

        public Task More
        {
            get { return _more; }
            private set { SetProperty(ref _more, value); }
        }

        public BranchesViewModel(string username, string repository)
        {
            Username = username;
            Repository = repository;
            _items = new CustomObservableCollection<BranchModel>();
        }

        public override async Task Load(bool forceDataRefresh)
        {
            await Task.Run(() => this.RequestModel(Application.Client.Users[Username].Repositories[Repository].GetBranches(), forceDataRefresh, response => {
                Items.Reset(response.Data);
                this.CreateMore(response, m => More = m, d => Items.AddRange(d));
            }));
        }
    }
}

