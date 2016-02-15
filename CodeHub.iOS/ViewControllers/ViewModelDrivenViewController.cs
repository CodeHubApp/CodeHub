using System;
using CodeHub.iOS.Utilities;
using CodeHub.Core.ViewModels;
using UIKit;
using MvvmCross.iOS.Views;

namespace CodeHub.iOS.ViewControllers
{
    public class ViewModelDrivenViewController : MvxViewController
    {
        private readonly UIBarButtonItem _backButton = new UIBarButtonItem { Image = Theme.CurrentTheme.BackButton };

        public ViewModelDrivenViewController()
        {
            NavigationItem.LeftBarButtonItem = _backButton;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _backButton.Clicked -= BackButtonClicked;
        }

        void BackButtonClicked (object sender, System.EventArgs e)
        {
            NavigationController.PopViewController(true);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var hud = new Hud(View);

            var loadableViewModel = ViewModel as LoadableViewModel;
            if (loadableViewModel != null)
            {

                loadableViewModel.Bind(x => x.IsLoading).Subscribe(x =>
                {
                    if (x)
                    {
                        NetworkActivity.PushNetworkActive();
                        hud.Show("Loading...");

                        if (ToolbarItems != null)
                        {
                            foreach (var t in ToolbarItems)
                                t.Enabled = false;
                        }
                    }
                    else
                    {
                        NetworkActivity.PopNetworkActive();
                        hud.Hide();

                        if (ToolbarItems != null)
                        {
                            foreach (var t in ToolbarItems)
                                t.Enabled = true;
                        }
                    }
                });
            }
        }

        bool _isLoaded = false;
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (!_isLoaded)
            {
                var loadableViewModel = ViewModel as LoadableViewModel;
                if (loadableViewModel != null)
                    loadableViewModel.LoadCommand.Execute(false);
                _isLoaded = true;
            }

            _backButton.Clicked += BackButtonClicked;
        }
    }
}

