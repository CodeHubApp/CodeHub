using System;
using CodeHub.Core.ViewModels.Source;
using MonoTouch.UIKit;
using ReactiveUI;
using CodeHub.iOS.ViewControllers;
using System.Reactive.Linq;

namespace CodeHub.iOS.Views.Source
{
    public class EditSourceView : MessageComposerViewController<EditSourceViewModel>
    {
		public EditSourceView()
		{
			EdgesForExtendedLayout = UIRectEdge.None;
            TextView.Font = UIFont.FromName("Courier", 12f);
            TextView.Changed += (sender, e) => ViewModel.Text = Text;
            this.WhenViewModel(x => x.Text).IsNotNull().Take(1).Subscribe(x => Text = x);
            this.WhenViewModel(x => x.Text).IsNotNull().Skip(1).Where(x => !string.Equals(x, TextView.Text)).Subscribe(x => TextView.Text = x);
            this.WhenViewModel(x => x.SaveCommand).Subscribe(x =>
                NavigationItem.RightBarButtonItem = new UIBarButtonItem(Images.SaveButton, UIBarButtonItemStyle.Plain, (s, e) => x.ExecuteIfCan()));
		}
    }
}

