using System;
using ReactiveUI;
using System.Threading.Tasks;
using System.Reactive;

namespace CodeHub.Core.ViewModels.Gists
{
    public abstract class GistFileModifyViewModel : BaseViewModel
    {
        private string _filename;
        public string Filename
        {
            get { return _filename; }
            set { this.RaiseAndSetIfChanged(ref _filename, value); }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { this.RaiseAndSetIfChanged(ref _description, value); }
        }

        public IReactiveCommand<Unit> SaveCommand { get; private set; }

        protected GistFileModifyViewModel(Func<Tuple<string, string>, Task> saveFunc)
        {
            var validObservable = this.WhenAnyValue(x => x.Filename, x => x.Description, (x, y) => 
                !string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(y));
            SaveCommand = ReactiveCommand.CreateAsyncTask(validObservable, 
                t => saveFunc(Tuple.Create(Filename, Description)));
            SaveCommand.Subscribe(_ => Dismiss());
        }
    }
}

