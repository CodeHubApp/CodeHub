using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;

namespace CodeHub.Core
{
    public class UserError
    {
        public string Title { get; }
        public string Message { get; }

        public UserError(string message, Exception exception = null)
            : this("Error", message, exception)
        {
        }
        
        public UserError(string title, string message, Exception exception = null)
        {
            Title = title;

            var messages = new List<string> { message };

            if (exception is TaskCanceledException)
                messages.Add("The request timed out waiting for the server to respond.");
            else
                messages.Add(exception.Message);

            Message = string.Join(" ", messages);
        }
    }

    public static class Interactions
    {
        public static readonly Interaction<UserError, Unit> Errors = new Interaction<UserError, Unit>();
    }
}
