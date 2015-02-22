using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace CodeHub.Core.ViewModels.Gists
{
    public class GistFileAddViewModel : GistFileModifyViewModel
    {
        public GistFileAddViewModel(Func<Tuple<string, string>, Task> saveFunc)
            : base(saveFunc)
        {
            this.WhenAnyValue(x => x.Filename)
                .Select(x => string.IsNullOrEmpty(x) ? "Add File" : x)
                .Subscribe(x => Title = x);
        }
    }
}

