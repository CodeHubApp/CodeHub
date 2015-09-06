
namespace System.Threading.Tasks
{
    public static class FireAndForgetTask
    {
        public static Task FireAndForget(this Task task)
        {
            return task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    var aggException = t.Exception.Flatten();
                    foreach(var exception in aggException.InnerExceptions)
                        System.Diagnostics.Debug.WriteLine("Fire and Forget failed: " + exception.Message + " - " + exception.StackTrace);
                }
                else if (t.IsCanceled)
                {
                    System.Diagnostics.Debug.WriteLine("Fire and forget canceled.");
                }
            });
        }
    }
}

