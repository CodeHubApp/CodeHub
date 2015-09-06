using System;
using UIKit;
using CoreGraphics;
using CoreGraphics;

namespace MonoTouch.Dialog
{
	
	/// <summary>
	/// GlassButton is a glossy/glass button.   User code can use either
	/// targets or can subscribe to the Tapped event.  Colors are customized
	/// by asssigning to the NormalColor, HighlightedColor and DisabledColor
	/// properties
	/// </summary>
	public class GlassButton : UIButton {
		bool pressed;
		
		public UIColor NormalColor, HighlightedColor, DisabledColor;
		
		/// <summary>
		/// Invoked when the user touches 
		/// </summary>
		public event Action<GlassButton> Tapped;
				
		/// <summary>
		/// Creates a new instance of the GlassButton using the specified dimensions
		/// </summary>
		public GlassButton (CGRect frame) : base (frame)
		{
			NormalColor = new UIColor (0.55f, 0.04f, 0.02f, 1);
			HighlightedColor = UIColor.Black;
			DisabledColor = UIColor.Gray;
		}

		/// <summary>
		/// Whether the button is rendered enabled or not.
		/// </summary>
		public override bool Enabled { 
			get {
				return base.Enabled;
			}
			set {
				base.Enabled = value;
				SetNeedsDisplay ();
			}
		}
		
		public override bool BeginTracking (UITouch uitouch, UIEvent uievent)
		{
			SetNeedsDisplay ();
			pressed = true;
			return base.BeginTracking (uitouch, uievent);
		}
		
		public override void EndTracking (UITouch uitouch, UIEvent uievent)
		{
			if (pressed && Enabled){
				if (Tapped != null)
					Tapped (this);
			}
			pressed = false;
			SetNeedsDisplay ();
			base.EndTracking (uitouch, uievent);
		}
		
		public override bool ContinueTracking (UITouch uitouch, UIEvent uievent)
		{
			var touch = uievent.AllTouches.AnyObject as UITouch;
			if (Bounds.Contains (touch.LocationInView (this)))
				pressed = true;
			else
				pressed = false;
			return base.ContinueTracking (uitouch, uievent);
		}
		
		public override void Draw (CGRect rect)
		{
			var context = UIGraphics.GetCurrentContext ();
			var bounds = Bounds;
			
			UIColor background = Enabled ? pressed ? HighlightedColor : NormalColor : DisabledColor;
			float alpha = 1;
			
			CGPath container;
			container = GraphicsUtil.MakeRoundedRectPath (bounds, 14);
			context.AddPath (container);
			context.Clip ();
			
			using (var cs = CGColorSpace.CreateDeviceRGB ()){
				var topCenter = new CGPoint (bounds.GetMidX (), 0);
				var midCenter = new CGPoint (bounds.GetMidX (), bounds.GetMidY ());
				var bottomCenter = new CGPoint (bounds.GetMidX (), bounds.GetMaxY ());

                using (var gradient = new CGGradient (cs, new nfloat [] { 0.23f, 0.23f, 0.23f, alpha, 0.47f, 0.47f, 0.47f, alpha }, new nfloat [] {0, 1})){
					context.DrawLinearGradient (gradient, topCenter, bottomCenter, 0);
				}
				
				container = GraphicsUtil.MakeRoundedRectPath (bounds.Inset (1, 1), 13);
				context.AddPath (container);
				context.Clip ();
                using (var gradient = new CGGradient (cs, new nfloat [] { 0.05f, 0.05f, 0.05f, alpha, 0.15f, 0.15f, 0.15f, alpha}, new nfloat [] {0, 1})){
					context.DrawLinearGradient (gradient, topCenter, bottomCenter, 0);
				}
				
				var nb = bounds.Inset (4, 4);
				container = GraphicsUtil.MakeRoundedRectPath (nb, 10);
				context.AddPath (container);
				context.Clip ();
				
				background.SetFill ();
				context.FillRect (nb);
				
                using (var gradient = new CGGradient (cs, new nfloat [] { 1, 1, 1, .35f, 1, 1, 1, 0.06f }, new nfloat [] { 0, 1 })){		
					
					context.DrawLinearGradient (gradient, topCenter, midCenter, 0);
				}
				context.SetLineWidth (2);
				context.AddPath (container);
				context.ReplacePathWithStrokedPath ();
				context.Clip ();

                using (var gradient = new CGGradient (cs, new nfloat [] { 1, 1, 1, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f }, new nfloat [] { 0, 1 })){
					context.DrawLinearGradient (gradient, topCenter, bottomCenter, 0);
				}
			}
		}
	}
}

