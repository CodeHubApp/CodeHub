using MvvmCross.Plugins.Messenger;

namespace CodeHub.Core.Messages
{
    public class CancelationMessage : MvxMessage
    {
        public CancelationMessage(object sender) 
            : base(sender)
        {
        }
    }
}

