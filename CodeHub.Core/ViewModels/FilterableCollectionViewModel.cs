using CodeHub.Core.Data;
using CodeHub.Core.Services;
using ReactiveUI;
using Splat;

namespace CodeHub.Core.ViewModels
{
    public class FilterableCollectionViewModel<T, TF> : CollectionViewModel<T>, IFilterableViewModel<TF> where TF : FilterModel<TF>, new()
    {
        protected TF _filter;
        private readonly string _filterKey;

        public TF Filter
        {
            get { return _filter; }
            set { this.RaiseAndSetIfChanged(ref _filter, value); }
        }

        public FilterableCollectionViewModel(string filterKey)
        {
            _filterKey = filterKey;
            var application = Locator.Current.GetService<IApplicationService>();
            var accounts = Locator.Current.GetService<IAccountsService>();
            _filter = application.Account.GetFilter<TF>(_filterKey) ?? new TF();
            accounts.Save(application.Account).ToBackground();
        }

        public void ApplyFilter(TF filter, bool saveAsDefault = false)
        {
            Filter = filter;
            if (saveAsDefault)
            {
                var application = Locator.Current.GetService<IApplicationService>();
                application.Account.SetFilter(_filterKey, _filter);
                application.UpdateActiveAccount().ToBackground();
            }
        }
    }
}

