using ReactiveUI;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CodeHub.Core.Services
{
    public interface IActionMenuService
    {
        IActionMenu Create(string title);

        IPickerMenu CreatePicker();
    }

    public interface IActionMenu
    {
        void AddButton(string title, IReactiveCommand clickAction);

        Task Show();
    }

    public interface IPickerMenu
    {
        ICollection<string> Options { get; }

        int SelectedOption { get; set; }

        Task<int> Show();
    }
}

