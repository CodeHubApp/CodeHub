using System;
using Xamarin.Utilities.Core.ViewModels;
using Xamarin.Utilities.Core;
using MonoTouch.UIKit;
using System.Reactive.Linq;
using System.Drawing;

namespace ReactiveUI
{
    public abstract class ReactiveTableViewController<TViewModel> : ReactiveTableViewController, IViewFor<TViewModel> where TViewModel : class
    {
        private TViewModel _viewModel;
        public TViewModel ViewModel
        {
            get { return _viewModel; }
            set { this.RaiseAndSetIfChanged(ref _viewModel, value); }
        }

        object IViewFor.ViewModel
        {
            get { return _viewModel; }
            set { ViewModel = (TViewModel)value; }
        }

        public override void LoadView()
        {
            base.LoadView();

            NavigationItem.BackBarButtonItem = new UIBarButtonItem { Title = string.Empty };

            this.WhenActivated(d =>
            {
                // Always keep this around since it calls the VM WhenActivated
            });

            var providesTitle = ViewModel as IProvidesTitle;
            if (providesTitle != null)
                providesTitle.WhenAnyValue(x => x.Title).Subscribe(x => Title = x ?? string.Empty);

            CreateSearchBar();
            LoadViewModel();
        }

        protected virtual void LoadViewModel()
        {
            var iLoadableViewModel = ViewModel as ILoadableViewModel;
            if (iLoadableViewModel != null)
            {
                var activityView = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.White) { 
                    Frame = new RectangleF(0, 0, 320f, 88f),
                    Color = NavigationController.NavigationBar.BackgroundColor,
                };
                activityView.StartAnimating();
                TableView.TableFooterView = activityView;

                iLoadableViewModel.LoadCommand.IsExecuting.Where(x => !x).Skip(1).Take(1).Subscribe(_ =>
                {
                    TableView.TableFooterView = null;
                    activityView.StopAnimating();

                    CreateRefreshControl();

                });
                iLoadableViewModel.LoadCommand.ExecuteIfCan();
            }

        }

        protected virtual void CreateRefreshControl()
        {
            var iLoadableViewModel = ViewModel as ILoadableViewModel;
            if (iLoadableViewModel != null)
                RefreshControl = ((ILoadableViewModel)ViewModel).LoadCommand.ToRefreshControl();
        }

        protected virtual void CreateSearchBar()
        {
            var searchableViewModel = ViewModel as IProvidesSearchKeyword;
            if (searchableViewModel != null)
                this.AddSearchBar(x => searchableViewModel.SearchKeyword = x);
        }

//        public class SpinnerView : UIView
//        {
//            readonly UIActivityIndicatorView _activity;
//
//            public SpinnerView()
//                : base(new RectangleF(0, 0, 320f, 44f))
//            {
//                _activity = new UIActivityIndicatorView(new RectangleF(0, 0, 20f, 20f));
//                _activity.Center = this.Center;
//                _activity.StartAnimating();
//                _activity.Color = UIColor.Black;
//                Add(_activity);
//            }
//        }
    }
}

