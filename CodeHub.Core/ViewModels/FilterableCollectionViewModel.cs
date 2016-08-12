using CodeHub.Core.Data;
using CodeHub.Core.Services;
using MvvmCross.Platform;

namespace CodeHub.Core.ViewModels
{
    public class FilterableCollectionViewModel<T, TF> : CollectionViewModel<T>, IFilterableViewModel<TF> where TF : FilterModel<TF>, new()
    {
        protected TF _filter;
        private readonly string _filterKey;

        public TF Filter
        {
            get { return _filter; }
            set
            {
                _filter = value;
                RaisePropertyChanged(() => Filter);
            }
        }

        public FilterableCollectionViewModel(string filterKey)
        {
            _filterKey = filterKey;
            var application = Mvx.Resolve<IApplicationService>();
            var accounts = Mvx.Resolve<IAccountsService>();
            _filter = application.Account.GetFilter<TF>(_filterKey) ?? new TF();
            accounts.Save(application.Account).ToBackground();
        }

        public void ApplyFilter(TF filter, bool saveAsDefault = false)
        {
            Filter = filter;
            if (saveAsDefault)
            {
                var application = Mvx.Resolve<IApplicationService>();
                application.Account.SetFilter(_filterKey, _filter);
                application.UpdateActiveAccount().ToBackground();
            }
        }
    }
}

