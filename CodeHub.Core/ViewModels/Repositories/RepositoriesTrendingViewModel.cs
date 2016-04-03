using System;
using CodeHub.Core.ViewModels;
using System.Windows.Input;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using System.Collections.Generic;
using CodeHub.Core.Data;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoriesTrendingViewModel : LoadableViewModel
    {
        private readonly Language _defaultLanguage = new Language("All Languages", null);

        private IList<Tuple<string, IList<RepositoryModel>>> _repos;
        public IList<Tuple<string, IList<RepositoryModel>>> Repositories
        {
            get { return _repos; }
            private set { this.RaiseAndSetIfChanged(ref _repos, value); }
        }

        private Language _selectedLanguage;
        public Language SelectedLanguage
        {
            get { return _selectedLanguage; }
            set { this.RaiseAndSetIfChanged(ref _selectedLanguage, value); }
        }

        public bool ShowRepositoryDescription
        {
            get { return this.GetApplication().Account.ShowRepositoryDescriptionInList; }
        }

        public RepositoriesTrendingViewModel()
        {
            SelectedLanguage = _defaultLanguage;
        }

        public void Init()
        {
            this.Bind(x => x.SelectedLanguage).Subscribe(_ => LoadCommand.Execute(null));
        }

        public ICommand GoToRepositoryCommand
        {
            get { return new MvxCommand<RepositoryModel>(x => ShowViewModel<RepositoryViewModel>(new RepositoryViewModel.NavObject { Username = x.Owner?.Login, Repository = x.Name })); }
        }
 
        protected override async Task Load()
        {
            var trendingRepo = new TrendingRepository();
            var repos = new List<Tuple<string, IList<RepositoryModel>>>();
            var times = new []
            {
                Tuple.Create("Daily", "daily"),
                Tuple.Create("Weekly", "weekly"),
                Tuple.Create("Monthly", "monthly"),
            };

            foreach (var t in times)
            {
                var repo = await trendingRepo.GetTrendingRepositories(t.Item2, SelectedLanguage.Slug);
                repos.Add(Tuple.Create(t.Item1, repo));
            }

            Repositories = repos;
        }
    }
}

