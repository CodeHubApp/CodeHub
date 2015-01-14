// Analysis disable once CheckNamespace
namespace ReactiveUI
{
    public class GroupedCollection<TViewModel>
    {
        public string Name { get; private set; }

        public IReadOnlyReactiveList<TViewModel> Items { get; private set; }

        public GroupedCollection(string name, IReadOnlyReactiveList<TViewModel> items)
        {
            Name = name;
            Items = items;
        }
    }

}