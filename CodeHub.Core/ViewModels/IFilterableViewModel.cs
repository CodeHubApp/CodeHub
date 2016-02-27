namespace CodeHub.Core.ViewModels
{
    public interface IFilterableViewModel<TFilter> where TFilter : FilterModel<TFilter>, new()
    {
        TFilter Filter { get; }

        void ApplyFilter(TFilter filter, bool saveAsDefault = false);
    }
}

