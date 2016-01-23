namespace System
{
	public static class ExceptionExtensions
    {
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Dump(this Exception e)
        {
            System.Diagnostics.Debug.WriteLine(e.Message + " - " + e.StackTrace);
        }
    }
}

