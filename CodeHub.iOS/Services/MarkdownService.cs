using MonoTouch.Foundation;
using MonoTouch.JavaScriptCore;
using CodeHub.Core.Services;

namespace CodeHub.iOS.Services
{
	public class MarkdownService : IMarkdownService
    {
		private readonly JSVirtualMachine _vm = new JSVirtualMachine();
		private readonly JSContext _ctx;
		private readonly JSValue _val;

		public MarkdownService()
		{
            var scriptPath = System.IO.Path.Combine(NSBundle.MainBundle.BundlePath, "WebResources", "marked.js");
            var scriptContents = System.IO.File.ReadAllText(scriptPath);

            _ctx = new JSContext(_vm);
            _ctx.EvaluateScript(scriptContents);
			_val = _ctx[new NSString("marked")];
		}

		public string Convert(string c)
		{
			if (string.IsNullOrEmpty(c))
				return string.Empty;
			return _val.Call(JSValue.From(c, _ctx)).ToString();
		}
    }
}

