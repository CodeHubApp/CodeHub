using System;
using System.Linq;
using ReactiveUI;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Octokit;

namespace CodeHub.Core.ViewModels.Gists
{
    public abstract class GistModifyViewModel : BaseViewModel
    {
        protected readonly ReactiveList<Tuple<string, string>> InternalFiles = new ReactiveList<Tuple<string, string>>(); 

        private string _description;
        public string Description
        {
            get { return _description; }
            set { this.RaiseAndSetIfChanged(ref _description, value); }
        }

        public IReadOnlyReactiveList<GistFileItemViewModel> Files { get; private set; }

        public IReactiveCommand<object> AddGistFileCommand { get; private set; }

        public IReactiveCommand<Gist> SaveCommand { get; private set; }

        public IReactiveCommand<bool> DismissCommand { get; private set; }

        protected GistModifyViewModel()
        {
            Files = InternalFiles.CreateDerivedCollection(x => 
            {
                var item = new GistFileItemViewModel(x.Item1, x.Item2);
                item.EditCommand.Subscribe(_ => NavigateTo(new GistFileEditViewModel(y =>
                {
                    var i = InternalFiles.IndexOf(x);
                    InternalFiles.Remove(x);
                    InternalFiles.Insert(i, Tuple.Create(y.Item1.Trim(), y.Item2));
                    return Task.FromResult(0);
                })
                {
                    Filename = x.Item1,
                    Description = x.Item2
                }));
                item.DeleteCommand.Subscribe(_ => InternalFiles.Remove(x));
                return item;
            });

            var canSave = new BehaviorSubject<bool>(false);
            Files.IsEmptyChanged.Select(x => !x).Subscribe(canSave.OnNext);

            SaveCommand = ReactiveCommand.CreateAsyncTask(canSave, _ => SaveGist());
            SaveCommand.Subscribe(_ => Dismiss());

            AddGistFileCommand = ReactiveCommand.Create();
            AddGistFileCommand.Subscribe(_ => NavigateTo(new GistFileAddViewModel(x =>
            {
                if (Files.Any(y => y.Name == x.Item1))
                    throw new Exception("Gist already contains a file with that name!");
                InternalFiles.Add(Tuple.Create(x.Item1.Trim(), x.Item2));
                return Task.FromResult(0);
            })));

            DismissCommand = ReactiveCommand.CreateAsyncTask(t => Discard());
            DismissCommand.Where(x => x).Subscribe(_ => Dismiss());
        }

        protected abstract Task<Gist> SaveGist();

        protected abstract Task<bool> Discard();
    }
}

