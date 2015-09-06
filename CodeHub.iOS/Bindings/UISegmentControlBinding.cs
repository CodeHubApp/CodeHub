using System;
using UIKit;

namespace CodeFramework.iOS.Bindings
{
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

