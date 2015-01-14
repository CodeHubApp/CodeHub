using Splat;
using CodeHub.Core.Factories;

namespace CodeHub.iOS.Factories
{
    public static class Registrations
    {
        public static void InitializeFactories(this IMutableDependencyResolver resolverToUse)
        {
            resolverToUse.Register(() => new ActionMenuFactory(), typeof(IActionMenuFactory));
            resolverToUse.Register(() => new AlertDialogFactory(), typeof(IAlertDialogFactory));
            resolverToUse.Register(() => new MediaPickerFactory(), typeof(IMediaPickerFactory));
        }
    }
}

