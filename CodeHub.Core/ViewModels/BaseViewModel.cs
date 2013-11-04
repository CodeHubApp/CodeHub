using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels
{
    public class BaseViewModel : CodeFramework.Core.ViewModels.BaseViewModel
    {
        public IApplicationService Application
        {
            get { return GetService<IApplicationService>(); }
        }
    }
}
