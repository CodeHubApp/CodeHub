using Cirrious.CrossCore;
using CodeFramework.Core.Services;

namespace System
{
	public static class ExceptionExtensions
    {
		public static void Report(this Exception e)
		{
			var service = Mvx.Resolve<IErrorService>();
			if (service != null)
				service.ReportError(e);
		}

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Dump(this Exception e)
        {
            System.Diagnostics.Debug.WriteLine(e.Message + " - " + e.StackTrace);
        }
    }
}

