using System;
using Cirrious.MvvmCross.Plugins.Messenger;

namespace CodeFramework.Core.Messages
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

