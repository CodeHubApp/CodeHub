using ReactiveUI;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistFileItemViewModel : ReactiveObject
    {
        public string Name { get; private set; }

        public string Content { get; private set; }

        public IReactiveCommand<object> EditCommand { get; private set; }

        public IReactiveCommand<object> DeleteCommand { get; private set; }

        public GistFileItemViewModel(string name, string content)
        {
            Name = name;
            Content = content;
            EditCommand = ReactiveCommand.Create();
            DeleteCommand = ReactiveCommand.Create();
        }
    }
}

