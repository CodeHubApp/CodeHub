using System.Reactive;
using ReactiveUI;

namespace CodeHub.Core
{
    public class UserError
    {
        public string Title { get; }
        public string Message { get; }

        public UserError(string message)
            : this("Error", message)
        {
        }
        
        public UserError(string title, string message)
        {
            Title = title;
            Message = message;
        }
    }

    public static class Interactions
    {
        public static readonly Interaction<UserError, Unit> Errors = new Interaction<UserError, Unit>();
    }
}
