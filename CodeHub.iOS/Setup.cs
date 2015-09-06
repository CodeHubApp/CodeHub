// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the Setup type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using Cirrious.CrossCore.IoC;
using Cirrious.MvvmCross.Binding.BindingContext;
using CodeFramework.Core.ViewModels;
using CodeFramework.iOS;
using MonoTouch.Dialog;
using CodeFramework.iOS.Views;
using UIKit;

namespace CodeHub.iOS
{
    using Cirrious.MvvmCross.Touch.Platform;
    using Cirrious.MvvmCross.Touch.Views.Presenters;
    using Cirrious.MvvmCross.ViewModels;

    /// <summary>
    ///    Defines the Setup type.
    /// </summary>
    public class Setup : MvxTouchSetup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Setup"/> class.
        /// </summary>
        /// <param name="applicationDelegate">The application delegate.</param>
        /// <param name="presenter">The presenter.</param>
        public Setup(MvxApplicationDelegate applicationDelegate, IMvxTouchViewPresenter presenter)
            : base(applicationDelegate, presenter)
        {
        }

        protected override Assembly[] GetViewModelAssemblies()
        {
            var list = new List<Assembly>();
            list.AddRange(base.GetViewModelAssemblies());
			list.Add(typeof(BaseStartupViewModel).Assembly);
            return list.ToArray();
        }

        protected override Cirrious.CrossCore.Platform.IMvxTrace CreateDebugTrace()
        {
            #if DEBUG
            return base.CreateDebugTrace();
            #else
			return new Cirrious.CrossCore.Platform.MvxDebugOnlyTrace();
            #endif
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