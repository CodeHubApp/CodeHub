using System;
using System.Linq;
using CodeHub.Core.ViewModels;
using CodeHub.iOS.Utilities;
using CodeHub.iOS.ViewControllers;
using UIKit;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeHub.iOS.Services;
using CodeHub.iOS.DialogElements;
using Foundation;
using CoreGraphics;

namespace CodeHub.iOS.ViewControllers
{
    public abstract class ViewModelCollectionDrivenDialogViewController : ViewModelDrivenDialogViewController
    {
        private static NSObject _dumb = new NSObject();

        public Lazy<UIView> EmptyView { get; set; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name='push'>True if navigation controller should push, false if otherwise</param>
        protected ViewModelCollectionDrivenDialogViewController(bool push = true)
            : base(push, UITableViewStyle.Plain)
        {
            EnableSearch = true;
        }

        private void CreateEmptyHandler(bool x)
        {
            if (EmptyView == null)
            {
                return;
            }
            if (x)
            {
                if (!EmptyView.IsValueCreated)
                {
                    EmptyView.Value.Alpha = 0f;
                    TableView.AddSubview(EmptyView.Value);
                }

                EmptyView.Value.UserInteractionEnabled = true;
                EmptyView.Value.Frame = new CGRect(0, 0, TableView.Bounds.Width, TableView.Bounds.Height);
                TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
                TableView.BringSubviewToFront(EmptyView.Value);
                if (TableView.TableHeaderView != null)
                    TableView.TableHeaderView.Hidden = true;
                UIView.Animate(0.2f, 0f, UIViewAnimationOptions.AllowUserInteraction | UIViewAnimationOptions.CurveEaseIn | UIViewAnimationOptions.BeginFromCurrentState,
                    () => EmptyView.Value.Alpha = 1.0f, null);
            }
            else if (EmptyView.IsValueCreated)
            {
                EmptyView.Value.UserInteractionEnabled = false;
                if (TableView.TableHeaderView != null)
                    TableView.TableHeaderView.Hidden = false;
                TableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
                UIView.Animate(0.1f, 0f, UIViewAnimationOptions.AllowUserInteraction | UIViewAnimationOptions.CurveEaseIn | UIViewAnimationOptions.BeginFromCurrentState,
                    () => EmptyView.Value.Alpha = 0f, null);
            }
        }
    }
}

