using System;
using ReactiveUI;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistFileEditViewModel : GistFileModifyViewModel
    {
        public GistFileEditViewModel(Func<Tuple<string, string>, Task> saveFunc)
            : base(saveFunc)
        {
            this.WhenAnyValue(x => x.Filename)
                .Select(x => string.IsNullOrEmpty(x) ? "Edit File" : x)
                .Subscribe(x => Title = x);
        }
    }
}

