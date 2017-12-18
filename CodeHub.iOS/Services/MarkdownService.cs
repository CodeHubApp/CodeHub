using JavaScriptCore;
using Foundation;
using CodeHub.Core.Services;
using System.Threading.Tasks;

namespace CodeHub.iOS.Services
{
    public class MarkdownService : IMarkdownService
    {
        private readonly JSVirtualMachine _vm = new JSVirtualMachine();
        private readonly JSContext _ctx;
        private readonly JSValue _val;

        public MarkdownService()
        {
            _ctx = new JSContext(_vm);
            var script = System.IO.File.ReadAllText("WebResources/marked.js", System.Text.Encoding.UTF8);
            _ctx.EvaluateScript(script);
            _val = _ctx[new NSString("marked")];
        }

        public Task<string> Convert(string c)
        {
            if (string.IsNullOrEmpty(c))
                return Task.FromResult(string.Empty);
            return Task.Run(() => _val.Call(JSValue.From(c, _ctx)).ToString());
        }
    }
}

