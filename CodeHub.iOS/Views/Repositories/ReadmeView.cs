using CodeFramework.iOS.ViewControllers;
using CodeHub.Core.ViewModels.Repositories;

namespace CodeHub.iOS.Views.Repositories
{
    public class ReadmeView : WebViewController
    {
        public new ReadmeViewModel ViewModel
        {
            get { return (ReadmeViewModel) base.ViewModel; }
            set { base.ViewModel = value; }
        }

        public ReadmeView() 
            : base(true)
        {
            Title = "Readme";
            Web.ScalesPageToFit = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            ViewModel.Bind(x => x.Path, x => LoadFile(x));
        }
                
        protected override void Refresh()
        {
            if (Web.Request.Url.AbsoluteString.Equals(ViewModel.Path))
            {
                if (RefreshButton != null)
                    RefreshButton.Enabled = false;
//
//                try
//                {
//                    await this.DoWorkNoHudAsync(async () => {
//                        await Task.Run(() => RequestAndSave(true));
//                    });
//                }
//                catch (Exception e)
//                {
//                    if (_isVisible)
//                        Utilities.ShowAlert("Unable to Refresh!", e.Message);
//                    if (RefreshButton != null)
//                        RefreshButton.Enabled = true;
//                    return;
//                }
            }


            base.Refresh();
        }
    }
}

