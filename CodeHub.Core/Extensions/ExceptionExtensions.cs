namespace System
{
    public static class ExceptionExtensions
    {
        public static Exception GetInnerException(this Exception This)
        {
            var ex = This;
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex;
        }
    }
}
