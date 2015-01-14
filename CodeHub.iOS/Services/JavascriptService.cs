using MonoTouch.JavaScriptCore;
using CodeHub.Core.Services;

namespace CodeHub.iOS.Services
{
    public class JavascriptService : IJavascriptService
    {
        public IJavascriptInstance CreateInstance()
        {
            return new JavascriptInstance(new JSVirtualMachine());
        }
    }

    public class JavascriptInstance : IJavascriptInstance
    {
        private readonly JSContext _ctx;

        public JavascriptInstance(JSVirtualMachine jsVirtualMachine)
        {
            _ctx = new JSContext(jsVirtualMachine);
        }

        public string Execute(string script)
        {
            return _ctx.EvaluateScript(script).ToString();
        }
    }


}