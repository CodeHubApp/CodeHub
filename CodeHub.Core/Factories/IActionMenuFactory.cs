using ReactiveUI;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace CodeHub.Core.Factories
{
    public interface IActionMenuFactory
    {
        IActionMenu Create(string title);

        IPickerMenu CreatePicker();

        Task ShareUrl(Uri uri);

        void SendToPasteBoard(string str);
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

    public static class ActionMenuFactoryExtensions
    {
        public static Task ShareUrl(this IActionMenuFactory @this, string uri)
        {
            return @this.ShareUrl(new Uri(uri));
        }
    }
}

