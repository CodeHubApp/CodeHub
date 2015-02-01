using Splat;
using CodeHub.Core.Services;
using Xamarin.Utilities.Services;

namespace CodeHub.iOS.Services
{
    public static class Registrations
    {
        public static void InitializeServices(this IMutableDependencyResolver resolverToUse)
        {
            resolverToUse.RegisterLazySingleton(() => new ViewModelViewService(), typeof(IViewModelViewService));
            resolverToUse.RegisterLazySingleton(() => new DefaultValueService(), typeof(IDefaultValueService));
            resolverToUse.RegisterLazySingleton(() => new GraphicService(), typeof(IGraphicService));
            resolverToUse.RegisterLazySingleton(() => new MarkdownService(), typeof(IMarkdownService));
            resolverToUse.RegisterLazySingleton(() => new ErrorService(), typeof(IErrorService));
            resolverToUse.RegisterLazySingleton(() => new ServiceConstructor(), typeof(IServiceConstructor));
            resolverToUse.RegisterLazySingleton(() => NetworkActivityService.Instance, typeof(INetworkActivityService));
            resolverToUse.RegisterLazySingleton(() => new FilesystemService(), typeof(IFilesystemService));
            resolverToUse.RegisterLazySingleton(() => new EnvironmentalService(), typeof(IEnvironmentalService));
            resolverToUse.RegisterLazySingleton(() => new UrlRouterService(resolverToUse.GetService<IAccountsService>()), typeof(IUrlRouterService));

            resolverToUse.RegisterLazySingleton(() => new TransitionOrchestrationService(resolverToUse.GetService<IViewModelViewService>(), 
                resolverToUse.GetService<IServiceConstructor>()), typeof(ITransitionOrchestrationService));
            resolverToUse.RegisterLazySingleton(() => new FeaturesService(resolverToUse.GetService<IDefaultValueService>()), typeof(IFeaturesService));
            resolverToUse.RegisterLazySingleton(() => new PushNotificationsService(resolverToUse.GetService<IApplicationService>()), typeof(IPushNotificationsService));
        }
    }
}

