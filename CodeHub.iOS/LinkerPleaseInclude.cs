// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the LinkerPleaseInclude type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace CodeHub.iOS
{
    using System;
    using System.Collections.Specialized;
    using System.Windows.Input;

    using MonoTouch.UIKit;

    /// <summary>
    /// Defines the LinkerPleaseInclude type.
    /// This class is never actually executed, but when Xamarin linking is enabled it does how to 
    /// ensure types and properties are preserved in the deployed app. 
    /// </summary>
    public class LinkerPleaseInclude
    {
        /// <summary>
        /// Includes the specified UI button.
        /// </summary>
        /// <param name="uiButton">The UI button.</param>
        public void Include(UIButton uiButton)
        {
            uiButton.TouchUpInside += (s, e) => uiButton.SetTitle(uiButton.Title(UIControlState.Normal), UIControlState.Normal);
        }

        /// <summary>
        /// Includes the specified bar button.
        /// </summary>
        /// <param name="barButton">The bar button.</param>
        public void Include(UIBarButtonItem barButton)
        {
            barButton.Clicked += (s, e) => barButton.Title = barButton.Title + string.Empty;
        }

        /// <summary>
        /// Includes the specified text field.
        /// </summary>
        /// <param name="textField">The text field.</param>
        public void Include(UITextField textField)
        {
            textField.Text = textField.Text + string.Empty;
            textField.EditingChanged += (sender, args) => { textField.Text = string.Empty; };
        }

        /// <summary>
        /// Includes the specified text view.
        /// </summary>
        /// <param name="uiTextView">The text view.</param>
        public void Include(UITextView uiTextView)
        {
            uiTextView.Text = uiTextView.Text + string.Empty;
            uiTextView.Changed += (sender, args) => { uiTextView.Text = string.Empty; };
        }

        /// <summary>
        /// Includes the specified uiLabel.
        /// </summary>
        /// <param name="uiLabel">The uiLabel.</param>
        public void Include(UILabel uiLabel)
        {
            uiLabel.Text = string.Format("{0}", uiLabel.Text);
        }

        /// <summary>
        /// Includes the specified image view.
        /// </summary>
        /// <param name="uiImageView">The image view.</param>
        public void Include(UIImageView uiImageView)
        {
            uiImageView.Image = new UIImage(uiImageView.Image.CIImage);
        }

        /// <summary>
        /// Includes the specified uiDatePicker.
        /// </summary>
        /// <param name="uiDatePicker">The uiDatePicker.</param>
        public void Include(UIDatePicker uiDatePicker)
        {
            uiDatePicker.Date = uiDatePicker.Date.AddSeconds(1);
            uiDatePicker.ValueChanged += (sender, args) => { uiDatePicker.Date = DateTime.MaxValue; };
        }

        /// <summary>
        /// Includes the specified uiSlider.
        /// </summary>
        /// <param name="uiSlider">The uiSlider.</param>
        public void Include(UISlider uiSlider)
        {
            uiSlider.Value = uiSlider.Value + 1;
            uiSlider.ValueChanged += (sender, args) => { uiSlider.Value = 1; };
        }

        /// <summary>
        /// Includes the specified UI switch.
        /// </summary>
        /// <param name="uiSwitch">The UI switch.</param>
        public void Include(UISwitch uiSwitch)
        {
            uiSwitch.On = !uiSwitch.On;
            uiSwitch.ValueChanged += (sender, args) => { uiSwitch.On = false; };
        }

        /// <summary>
        /// Includes the specified changed.
        /// </summary>
        /// <param name="changed">The changed.</param>
        public void Include(INotifyCollectionChanged changed)
        {
            changed.CollectionChanged += (s, e) => { string test = string.Format("{0}{1}{2}{3}{4}", e.Action, e.NewItems, e.NewStartingIndex, e.OldItems, e.OldStartingIndex); };
        }

        /// <summary>
        /// Includes the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void Include(ICommand command)
        {
            command.CanExecuteChanged += (s, e) => { if (command.CanExecute(null)) command.Execute(null); };
        }
    }
}
