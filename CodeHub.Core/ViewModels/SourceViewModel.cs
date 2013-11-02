using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeFramework.Core.ViewModels;
using CodeHub.Core.Filters;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels
{
    public class SourceViewModel : BaseViewModel, ILoadableViewModel
    {
        private readonly FilterableCollectionViewModel<ContentModel, SourceFilterModel> _content;
        private SourceFilterModel _filter;

        public FilterableCollectionViewModel<ContentModel, SourceFilterModel> Content
        {
            get { return _content; }
        }

        public string Username
        {
            get;
            private set;
        }

        public string Path
        {
            get;
            private set;
        }

        public string Branch
        {
            get;
            private set;
        }

        public string Repository
        {
            get;
            private set;
        }

        public SourceFilterModel Filter
        {
            get { return _filter; }
            set
            {
                _filter = value;
                RaisePropertyChanged(() => Filter);
                _content.Refresh();
            }
        }

        public SourceViewModel()
        {
            _content = new FilterableCollectionViewModel<ContentModel, SourceFilterModel>("SourceViewModel");
            _content.FilteringFunction = FilterModel;
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
            Branch = navObject.Branch ?? "master";
            Path = navObject.Path ?? "";
        }

        private IEnumerable<ContentModel> FilterModel(IEnumerable<ContentModel> model)
        {
            IEnumerable<ContentModel> ret = model;
            var order = (SourceFilterModel.Order)_content.Filter.OrderBy;
            if (order == SourceFilterModel.Order.Alphabetical)
                ret = model.OrderBy(x => x.Name);
            else if (order == SourceFilterModel.Order.FoldersThenFiles)
                ret = model.OrderBy(x => x.Type).ThenBy(x => x.Name);
            return _content.Filter.Ascending ? ret : ret.Reverse();
        }

        public Task Load(bool forceDataRefresh)
        {
            return Content.SimpleCollectionLoad(Application.Client.Users[Username].Repositories[Repository].GetContent(Path, Branch), forceDataRefresh);
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
            public string Branch { get; set; }
            public string Path { get; set; }
        }
    }
}

