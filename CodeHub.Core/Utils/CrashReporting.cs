using System;
using System.Runtime.InteropServices;

namespace CodeFramework.Core.Utils
{
	public static class CrashReporting
	{
		[DllImport ("libc")]
		private static extern int sigaction (Signal sig, IntPtr act, IntPtr oact);

		enum Signal {
			SIGBUS = 10,
			SIGSEGV = 11
		}

		/// <summary>
		/// If you're using a crash reporter that isn't aware of Mono, pass its initialization code as a parameter to this method.
		/// Otherwise, null exceptions may crash your app.
		/// </summary>
		/// <example>
		/// <code>
		/// CrashReporting.HookCrashReporterWithMono (() => {
		///     // Using HockeyApp in this example
		///     var manager = BITHockeyManager.SharedHockeyManager;
		///     manager.Configure (HockeyAppId, null);
		///     manager.StartManager ();
		/// });
		/// </code>
		///
		/// </example>
		/// <remarks>
		/// Learn more about the problem:
		/// http://stackoverflow.com/a/14499336/458193
		/// </remarks>
		public static void HookCrashReporterWithMono (Action crashReporterInitializationCode)
		{
			if (crashReporterInitializationCode == null)
				throw new ArgumentNullException ("crashReporterInitializationCode");

			// Store Mono SIGSEGV and SIGBUS handlers

			IntPtr sigbus = Marshal.AllocHGlobal (512);
			IntPtr sigsegv = Marshal.AllocHGlobal (512);

			sigaction (Signal.SIGBUS, IntPtr.Zero, sigbus);
			sigaction (Signal.SIGSEGV, IntPtr.Zero, sigsegv);

			// Enable crash reporting

			crashReporterInitializationCode ();

			// Restore Mono SIGSEGV and SIGBUS handlers

			sigaction (Signal.SIGBUS, sigbus, IntPtr.Zero);
			sigaction (Signal.SIGSEGV, sigsegv, IntPtr.Zero);

			Marshal.FreeHGlobal (sigbus);
			Marshal.FreeHGlobal (sigsegv);
		}
	}
}

