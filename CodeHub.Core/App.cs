using Cirrious.MvvmCross.ViewModels;
using CodeHub.Core.ViewModels.App;
using CodeFramework.Core.Services;
using Cirrious.CrossCore;

namespace CodeHub.Core
{
    /// <summary>
    /// Define the App type.
    /// </summary>
    public class App : MvxApplication
    {
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public override void Initialize()
        {
			//Ensure this is loaded
			Cirrious.MvvmCross.Plugins.Messenger.PluginLoader.Instance.EnsureLoaded();

			var httpService = Mvx.Resolve<IHttpClientService>();
			var jsonSerializerService = Mvx.Resolve<IJsonSerializationService>();
			GitHubSharp.Client.ClientConstructor = httpService.Create;
			GitHubSharp.Client.Serializer = new GitHubSharpSerializer(jsonSerializerService);

            // Start the app with the First View Model.
			this.RegisterAppStart<StartupViewModel>();
        }

		class GitHubSharpSerializer : GitHubSharp.ISerializer
		{
			readonly IJsonSerializationService _service;
			public GitHubSharpSerializer(IJsonSerializationService service)
			{
				this._service = service;
			}
			public string Serialize(object o)
			{
				return _service.Serialize(o);
			}
			public TData Deserialize<TData>(string data)
			{
				return _service.Deserialize<TData>(data);
			}
		}
    }
}