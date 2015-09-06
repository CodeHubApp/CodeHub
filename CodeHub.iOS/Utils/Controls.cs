using System;
using CoreGraphics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using CoreGraphics;
using CoreAnimation;
using Foundation;
using UIKit;

namespace MonoTouch.Dialog
{
	public enum RefreshViewStatus {
		ReleaseToReload,
		PullToReload,
		Loading
	}

	// This cute method will be added to UIImage.FromResource, but for old installs 
	// make a copy here
	internal static class Util {
		public static UIImage FromResource (Assembly assembly, string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			assembly = Assembly.GetCallingAssembly ();
			var stream = assembly.GetManifestResourceStream (name);
			if (stream == null)
				return null;
			
			IntPtr buffer = Marshal.AllocHGlobal ((int) stream.Length);
			if (buffer == IntPtr.Zero)
				return null;
			
			var copyBuffer = new byte [Math.Min (1024, (int) stream.Length)];
			int n;
			IntPtr target = buffer;
			while ((n = stream.Read (copyBuffer, 0, copyBuffer.Length)) != 0){
				Marshal.Copy (copyBuffer, 0, target, n);
				target = (IntPtr) ((int) target + n);
			}
			try {
				var data = NSData.FromBytes (buffer, (uint) stream.Length);
				return UIImage.LoadFromData (data);
			} finally {
				Marshal.FreeHGlobal (buffer);
				stream.Dispose ();
			}
		}
	
	}
	
	public class RefreshTableHeaderView : UIView {
		static UIImage arrow = Util.FromResource (null, "arrow.png");
		protected UIActivityIndicatorView Activity;
		protected UILabel LastUpdateLabel, StatusLabel;
		protected UIImageView ArrowView;		
			
		public RefreshTableHeaderView (CGRect rect) : base (rect)
		{
			this.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			
			BackgroundColor = new UIColor (0.88f, 0.9f, 0.92f, 1);
			CreateViews ();
		}
		
		public virtual void CreateViews ()
		{
			LastUpdateLabel = new UILabel (){
				Font = UIFont.SystemFontOfSize (13f),
				TextColor = new UIColor (0.47f, 0.50f, 0.57f, 1),
				ShadowColor = UIColor.White, 
				ShadowOffset = new CGSize (0, 1),
				BackgroundColor = this.BackgroundColor,
				Opaque = true,
				TextAlignment = UITextAlignment.Center,
				AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleRightMargin
			};
			AddSubview (LastUpdateLabel);
			
			StatusLabel = new UILabel (){
				Font = UIFont.BoldSystemFontOfSize (14),
				TextColor = new UIColor (0.47f, 0.50f, 0.57f, 1),
				ShadowColor = LastUpdateLabel.ShadowColor,
				ShadowOffset = new CGSize (0, 1),
				BackgroundColor = this.BackgroundColor,
				Opaque = true,
				TextAlignment = UITextAlignment.Center,
				AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleRightMargin
			};
			AddSubview (StatusLabel);
			SetStatus (RefreshViewStatus.PullToReload);
			
			ArrowView = new UIImageView (){
				ContentMode = UIViewContentMode.ScaleAspectFill,
				Image = arrow,
				AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleRightMargin
			};
			ArrowView.Layer.Transform = CATransform3D.MakeRotation ((float) Math.PI, 0, 0, 1);
			AddSubview (ArrowView);
			
			Activity = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.Gray) {
				HidesWhenStopped = true,
				AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleRightMargin
			};
			AddSubview (Activity);
		}
		
		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			var bounds = Bounds;
			
			LastUpdateLabel.Frame = new CGRect (0, bounds.Height - 30, bounds.Width, 20);
			StatusLabel.Frame = new CGRect (0, bounds.Height-48, bounds.Width, 20);
			ArrowView.Frame = new CGRect (20, bounds.Height - 65, 30, 55);
			Activity.Frame = new CGRect (25, bounds.Height-38, 20, 20);
		}
		
		RefreshViewStatus status = (RefreshViewStatus) (-1);
		
		public virtual void SetStatus (RefreshViewStatus status)
		{
			if (this.status == status)
				return;
			
			string s = "Release to refresh".GetText ();
	
			switch (status){
			case RefreshViewStatus.Loading:
				s = "Loading...".GetText (); 
				break;
				
			case RefreshViewStatus.PullToReload:
				s = "Pull down to refresh...".GetText ();
				break;
			}
			StatusLabel.Text = s;
		}
		
		public override void Draw (CGRect rect)
		{
			var context = UIGraphics.GetCurrentContext ();
			context.DrawPath (CGPathDrawingMode.FillStroke);
			StatusLabel.TextColor.SetStroke ();
			context.BeginPath ();
			context.MoveTo (0, Bounds.Height-1);
			context.AddLineToPoint (Bounds.Width, Bounds.Height-1);
			context.StrokePath ();
		}		
		
		public bool IsFlipped;
		
		public void Flip (bool animate)
		{
			UIView.BeginAnimations (null);
			UIView.SetAnimationDuration (animate ? .18f : 0);
			ArrowView.Layer.Transform = IsFlipped 
				? CATransform3D.MakeRotation ((float)Math.PI, 0, 0, 1) 
				: CATransform3D.MakeRotation ((float)Math.PI * 2, 0, 0, 1);
				
			UIView.CommitAnimations ();
			IsFlipped = !IsFlipped;
		}
		
		DateTime lastUpdateTime;
		public DateTime LastUpdate {
			get {
				return lastUpdateTime;
			}
			set {
				if (value == lastUpdateTime)
					return;
				
				lastUpdateTime = value;
				if (value == DateTime.MinValue){
					LastUpdateLabel.Text = "Last Updated: never".GetText ();
				} else 
					LastUpdateLabel.Text = String.Format ("Last Updated: {0:d} {1}".GetText (), value, value.ToString("h:mm tt"));
			}
		}
		
		public void SetActivity (bool active)
		{
			if (active){
				Activity.StartAnimating ();
				ArrowView.Hidden = true;
				SetStatus (RefreshViewStatus.Loading);
			} else {
				Activity.StopAnimating ();
				ArrowView.Hidden = false;
			}
		}
	}
	
	public class SearchChangedEventArgs : EventArgs {
		public SearchChangedEventArgs (string text) 
		{
			Text = text;
		}
		public string Text { get; set; }
	}
}
