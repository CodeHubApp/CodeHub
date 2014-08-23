using CodeHub.Core.ViewModels.Users;
using ReactiveUI;
using System;
using MonoTouch.UIKit;
using System.Drawing;
using CodeHub.iOS.TableViewSources;

namespace CodeHub.iOS.Views.Users
{
    public abstract class BaseUserCollectionView<TViewModel> : ReactiveTableViewController<TViewModel> where TViewModel : BaseUserCollectionViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.Source = new UserTableViewSource(TableView, ViewModel.Users);
            ViewModel.LoadCommand.ExecuteIfCan();

//            TableView.Scrolled += (object sender, EventArgs e) => 
//            {
//                if ((TableView.ContentSize.Height - TableView.ContentOffset.Y) / TableView.ContentSize.Height < .3f)
//                {
//                    if (ViewModel.LoadMore != null)
//                    {
//                        ViewModel.LoadMore.ExecuteIfCan();
//                    }
//                }
//            };

//            ViewModel.WhenAnyValue(x => x.LoadMore)
//                .Subscribe(x =>
//            {
//                    if (x == null)
//                        TableView.TableFooterView = null;
//                    else
//                    {
//                        TableView.TableFooterView = new SpinnerView();
//                        //x.ExecuteIfCan();
//                    }
//            });



//            SearchTextChanging.Subscribe(x => ViewModel.SearchKeyword = x);
//
//            this.BindList(ViewModel.Users, x =>
//            {
//                var e = new UserElement(x.Login, string.Empty, string.Empty, x.AvatarUrl);
//                e.Tapped += () => ViewModel.GoToUserCommand.Execute(x);
//                return e;
//            });
        }
    }

    public class SpinnerView : UIView
    {
        readonly UIActivityIndicatorView _activity;

        public SpinnerView()
            : base(new RectangleF(0, 0, 320f, 44f))
        {
            _activity = new UIActivityIndicatorView(new RectangleF(0, 0, 20f, 20f));
            _activity.Center = this.Center;
            _activity.StartAnimating();
            _activity.Color = UIColor.Black;
            Add(_activity);
        }
    }



}

