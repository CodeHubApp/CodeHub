using System;
using MvvmCross.Platform.IoC;
using Foundation;

namespace CodeHub.iOS
{
    [Preserve(AllMembers = true)]
    public class LinkerPleaseInclude
    {
        public void Include(MvxPropertyInjector injector){
            injector = new MvxPropertyInjector ();
        } 
    }
}

