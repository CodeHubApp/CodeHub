using System;
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
            Message = exception == null ? message : $"{message} {ExceptionMessage(exception)}";
        }

        private static string ExceptionMessage(Exception exception)
        {
            if (exception is TaskCanceledException)
                return "The request timed out waiting for the server to respond.";
            else
                return exception?.Message;
        }
    }

    public static class Interactions
    {
        public static readonly Interaction<UserError, Unit> Errors = new Interaction<UserError, Unit>();
    }
}
