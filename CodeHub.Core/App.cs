using System.Linq;
using Cirrious.CrossCore.IoC;
using Cirrious.MvvmCross.ViewModels;
using CodeFramework.Core.ViewModels;

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
            //// Start the app with the First View Model.
            this.RegisterAppStart<AccountsViewModel>();
        }
    }
}