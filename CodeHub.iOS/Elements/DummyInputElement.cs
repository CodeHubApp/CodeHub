using System;
using Xamarin.Utilities.DialogElements;
using MonoTouch.UIKit;
using System.Drawing;

namespace CodeHub.iOS.Elements
{
    class DummyInputElement : InputElement
    {
        public bool SpellChecking { get; set; }

        public DummyInputElement(string name)
            : base(name, name, string.Empty)
        {
            SpellChecking = true;
        }

        protected override UITextField CreateTextField(RectangleF frame)
        {
            var txt = base.CreateTextField(frame);
            txt.AllEditingEvents += (sender, e) => FetchValue();
            txt.AutocorrectionType = SpellChecking ? UITextAutocorrectionType.Default : UITextAutocorrectionType.No;
            txt.SpellCheckingType = SpellChecking ? UITextSpellCheckingType.Default : UITextSpellCheckingType.No;
            txt.AutocapitalizationType = SpellChecking ? txt.AutocapitalizationType : UITextAutocapitalizationType.None;
            return txt;
        }
    }
}

