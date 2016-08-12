using MvvmCross.Platform.IoC;

namespace CodeHub.iOS
{
    [Preserve]
    public class LinkerPleaseInclude
    {
        public void Include(MvxPropertyInjector injector){
            injector = new MvxPropertyInjector ();
        } 
    }
}

