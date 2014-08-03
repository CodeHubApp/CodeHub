using Xamarin.Utilities.Core.Services;

namespace System
{
	public static class ExceptionExtensions
    {
		public static void Report(this Exception e)
		{
			var service = IoC.Resolve<IErrorService>();
			if (service != null)
				service.ReportError(e);
		}

        [Diagnostics.Conditional("DEBUG")]
        public static void Dump(this Exception e)
        {
            Diagnostics.Debug.WriteLine(e.Message + " - " + e.StackTrace);
        }
    }
}

