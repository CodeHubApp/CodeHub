using Cirrious.MvvmCross.Plugins.Messenger;

namespace CodeFramework.Core.Messages
{
    public class CancelationMessage : MvxMessage
    {
        public CancelationMessage(object sender) 
            : base(sender)
        {
        }
    }
}

