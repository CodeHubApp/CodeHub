using System;
using MvvmCross.Plugins.Messenger;

namespace CodeHub.Core.Messages
{
    public class ErrorMessage : MvxMessage
    {
        public Exception Error { get; private set; }

        public ErrorMessage(object sender, Exception error) 
            : base(sender)
        {
            Error = error;
        }
    }
}

