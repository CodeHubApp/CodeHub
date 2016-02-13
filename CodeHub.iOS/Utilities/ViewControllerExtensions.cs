using System;
using System.Threading;
using System.Threading.Tasks;
using UIKit;

namespace CodeHub.iOS.Utilities
{
    public static class ViewControllerExtensions
    {
		public static IHud CreateHud(this UIViewController controller)
		{
			return new Hud(controller.View);
		}

		public async static Task<T> DoWorkAsync<T>(this UIViewController controller, string workTitle, Func<Task<T>> work)
		{
			var hud = CreateHud(controller);
			hud.Show(workTitle);

			//Make sure the Toolbar is disabled too
			if (controller.ToolbarItems != null)
			{
				foreach (var t in controller.ToolbarItems)
					t.Enabled = false;
			}

			try
			{
				return await DoWorkNoHudAsync(controller, work);
			}
			finally
			{
				hud.Hide();

				//Enable all the toolbar items
				if (controller.ToolbarItems != null)
				{
					foreach (var t in controller.ToolbarItems)
						t.Enabled = true;
				}
			}
		}


        public async static Task DoWorkAsync(this UIViewController controller, string workTitle, Func<Task> work)
        {
			var hud = CreateHud(controller);
			hud.Show(workTitle);

            //Make sure the Toolbar is disabled too
            if (controller.ToolbarItems != null)
            {
                foreach (var t in controller.ToolbarItems)
                    t.Enabled = false;
            }

            try
            {
                await DoWorkNoHudAsync(controller, work);
            }
            finally
            {
                hud.Hide();

                //Enable all the toolbar items
                if (controller.ToolbarItems != null)
                {
                    foreach (var t in controller.ToolbarItems)
                        t.Enabled = true;
                }
            }
        }

		public async static Task<T> DoWorkNoHudAsync<T>(this UIViewController controller, Func<Task<T>> work)
		{
			try
			{
                NetworkActivity.PushNetworkActive();
				return await work();
			}
			catch (Exception e)
			{
				throw e;
			}
			finally 
			{
                NetworkActivity.PopNetworkActive();
			}
		}

        public async static Task DoWorkNoHudAsync(this UIViewController controller, Func<Task> work)
        {
            try
            {
                NetworkActivity.PushNetworkActive();
                await work();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally 
            {
                NetworkActivity.PopNetworkActive();
            }
        }

        public static void DoWorkNoHud(this UIViewController controller, Action work, Action<Exception> error = null, Action final = null)
        {
            ThreadPool.QueueUserWorkItem(delegate {
                try
                {
                    NetworkActivity.PushNetworkActive();
                    work();
                }
                catch (Exception e)
                {
                    if (error != null)
                        controller.InvokeOnMainThread(() => error(e));
                }
                finally 
                {
                    NetworkActivity.PopNetworkActive();
                    if (final != null)
                        controller.InvokeOnMainThread(() => final());
                }
            });
        }
    }
}

