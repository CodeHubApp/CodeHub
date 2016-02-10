// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the Setup type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection;
using MonoTouch.Dialog;
using UIKit;
using MvvmCross.iOS.Platform;
using MvvmCross.iOS.Views.Presenters;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platform.IoC;
using CodeHub.Core.ViewModels.App;

namespace CodeHub.iOS
{
    using MvvmCross.Core.ViewModels;

    /// <summary>
    ///    Defines the Setup type.
    /// </summary>
    public class Setup : MvxIosSetup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Setup"/> class.
        /// </summary>
        /// <param name="applicationDelegate">The application delegate.</param>
        /// <param name="presenter">The presenter.</param>
        public Setup(MvxApplicationDelegate applicationDelegate, IMvxIosViewPresenter presenter)
            : base(applicationDelegate, presenter)
        {
        }

        protected override IEnumerable<Assembly> GetViewModelAssemblies()
        {
            var list = new List<Assembly>();
            list.AddRange(base.GetViewModelAssemblies());
			list.Add(typeof(StartupViewModel).Assembly);
            return list.ToArray();
        }

        protected override void FillBindingNames(IMvxBindingNameRegistry obj)
        {
            base.FillBindingNames(obj);
			obj.AddOrOverwrite(typeof(StyledStringElement), "Tapped");
			obj.AddOrOverwrite(typeof(UISegmentedControl), "ValueChanged");
        }

        /// <summary>
        /// Creates the app.
        /// </summary>
        /// <returns>An instance of IMvxApplication</returns>
        protected override IMvxApplication CreateApp()
        {
            this.CreatableTypes(typeof(Core.App).Assembly)
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            this.CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            this.CreatableTypes()
                .EndingWith("Factory")
                .AsInterfaces()
                .RegisterAsDynamic();

            this.CreatableTypes(typeof(Core.App).Assembly)
                .EndingWith("Factory")
                .AsInterfaces()
                .RegisterAsDynamic();

            return new Core.App();
        }
    }
}