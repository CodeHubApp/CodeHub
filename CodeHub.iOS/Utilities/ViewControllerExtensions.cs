using System;
using System.Threading.Tasks;
using CodeHub.iOS.Services;
using UIKit;

namespace CodeHub.iOS.Utilities
{
    public static class ViewControllerExtensions
    {
        public static void ShowError(this UIViewController viewController, string title, Exception exception)
        {
            AlertDialogService.ShowAlert(title, exception.Message);
        }

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
                NetworkActivity.PushNetworkActive();
                return await work();
            }
            finally
            {
                NetworkActivity.PopNetworkActive();

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
                NetworkActivity.PushNetworkActive();
                await work();
            }
            finally
            {
                NetworkActivity.PopNetworkActive();

                hud.Hide();

                //Enable all the toolbar items
                if (controller.ToolbarItems != null)
                {
                    foreach (var t in controller.ToolbarItems)
                        t.Enabled = true;
                }
            }
        }
    }
}

