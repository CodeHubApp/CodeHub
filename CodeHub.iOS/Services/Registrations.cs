using Splat;
using Xamarin.Utilities.Services;
using CodeHub.Core.Services;

namespace CodeHub.iOS.Services
{
    public static class Registrations
    {
        public static void InitializeLocal(this IMutableDependencyResolver resolverToUse)
        {
            var serviceConstructor = resolverToUse.GetService<IServiceConstructor>();
            var viewModelViewService = resolverToUse.GetService<IViewModelViewService>();

            resolverToUse.RegisterLazySingleton(() => new TransitionOrchestrationService(viewModelViewService, serviceConstructor), typeof(ITransitionOrchestrationService));
            resolverToUse.RegisterLazySingleton(() => new FeaturesService(resolverToUse.GetService<IDefaultValueService>()), typeof(IFeaturesService));
            resolverToUse.RegisterLazySingleton(() => new GraphicService(), typeof(IGraphicService));
            resolverToUse.RegisterLazySingleton(() => new MarkdownService(), typeof(IMarkdownService));
            resolverToUse.RegisterLazySingleton(() => new PushNotificationsService(resolverToUse.GetService<IApplicationService>()), typeof(IPushNotificationsService));
        }
    }
}

