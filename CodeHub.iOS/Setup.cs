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
using MonoTouch.UIKit;

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

        protected override Assembly[] GetViewAssemblies()
        {
            var list = new List<Assembly>();
            list.AddRange(base.GetViewAssemblies());
            list.Add(typeof(StartupView).Assembly);
            return list.ToArray();
        }

        protected override Assembly[] GetViewModelAssemblies()
        {
            var list = new List<Assembly>();
            list.AddRange(base.GetViewModelAssemblies());
			list.Add(typeof(BaseStartupViewModel).Assembly);
            return list.ToArray();
        }
//
//        protected override IMvxTrace CreateDebugTrace()
//        {
//            return new ;
//        }

        protected override void FillBindingNames(IMvxBindingNameRegistry obj)
        {
            base.FillBindingNames(obj);
			obj.AddOrOverwrite(typeof(StyledStringElement), "Tapped");
			obj.AddOrOverwrite(typeof(UISegmentedControl), "ValueChanged");
        }

		protected override void FillTargetFactories(Cirrious.MvvmCross.Binding.Bindings.Target.Construction.IMvxTargetBindingFactoryRegistry registry)
		{
			base.FillTargetFactories(registry);
			registry.RegisterFactory(new Cirrious.MvvmCross.Binding.Bindings.Target.Construction.MvxCustomBindingFactory<UISegmentedControl>("ValueChanged", x => new UISegmentControlBinding(x)));
		}

        /// <summary>
        /// Creates the app.
        /// </summary>
        /// <returns>An instance of IMvxApplication</returns>
        protected override IMvxApplication CreateApp()
        {
            this.CreatableTypes(typeof(BaseViewModel).Assembly)
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            this.CreatableTypes(typeof(TouchViewPresenter).Assembly)
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            this.CreatableTypes(typeof(Core.App).Assembly)
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            this.CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();

            return new Core.App();
        }
    }

	public class UISegmentControlBinding : Cirrious.MvvmCross.Binding.Bindings.Target.MvxTargetBinding
	{
		private readonly UISegmentedControl _ctrl;

		public UISegmentControlBinding(UISegmentedControl ctrl)
			: base(ctrl)
		{
			this._ctrl = ctrl;
		}

		public override void SetValue(object value)
		{
			_ctrl.SelectedSegment = (int)value;
		}
		public override void SubscribeToEvents()
		{
			_ctrl.ValueChanged += HandleValueChanged;
		}

		void HandleValueChanged (object sender, EventArgs e)
		{
			FireValueChanged(_ctrl.SelectedSegment);
		}

		public override Type TargetType
		{
			get
			{
				return typeof(int);
			}
		}
		public override Cirrious.MvvmCross.Binding.MvxBindingMode DefaultMode
		{
			get
			{
				return Cirrious.MvvmCross.Binding.MvxBindingMode.TwoWay;
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				_ctrl.ValueChanged -= HandleValueChanged;
			}
			base.Dispose(isDisposing);
		}
	}
}