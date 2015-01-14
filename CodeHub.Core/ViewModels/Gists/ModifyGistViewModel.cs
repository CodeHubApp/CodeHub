using System;
using ReactiveUI;

namespace CodeHub.Core.ViewModels.Gists
{
    public class ModifyGistViewModel : BaseViewModel
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

        public new string Title
        {
            get { return base.Title; }
            set { base.Title = value; }
        }

        public IReactiveCommand<object> SaveCommand { get; private set; }

        public ModifyGistViewModel()
        {
            var validObservable = this.WhenAnyValue(x => x.Filename, x => x.Description, (x, y) => 
                !string.IsNullOrEmpty(x) && !string.IsNullOrEmpty(y));
            SaveCommand = ReactiveCommand.Create(validObservable);
        }

        public new void Dismiss()
        {
            base.Dismiss();
        }
    }
}

