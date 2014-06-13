using Xamarin.Utilities.Core.Services;

namespace CodeHub.Core
{
    /// <summary>
    /// Define the App type.
    /// </summary>
    public static class Bootstrap
    {
        public static void Init()
        {
            var httpService = IoC.Resolve<IHttpClientService>();
			GitHubSharp.Client.ClientConstructor = httpService.Create;
        }
    }
}