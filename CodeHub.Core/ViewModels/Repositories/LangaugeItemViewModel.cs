using ReactiveUI;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class LanguageItemViewModel : ReactiveObject
    {
        public string Name { get; }

        public string Slug { get; }

        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set { this.RaiseAndSetIfChanged(ref _selected, value); }
        }

        internal LanguageItemViewModel(string name, string slug)
        {
            Name = name;
            Slug = slug;
        }
    }
}

