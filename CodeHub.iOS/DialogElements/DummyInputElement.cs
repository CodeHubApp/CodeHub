using UIKit;
using CoreGraphics;
using CodeHub.iOS.DialogElements;

namespace CodeHub.iOS.DialogElements
{
    class DummyInputElement : EntryElement
    {
        public bool SpellChecking { get; set; }

        public DummyInputElement(string name)
            : base(name, name, string.Empty)
        {
            SpellChecking = true;
        }

        protected override UITextField CreateTextField(CGRect frame)
        {
            var txt = base.CreateTextField(frame);
            txt.AutocorrectionType = SpellChecking ? UITextAutocorrectionType.Default : UITextAutocorrectionType.No;
            txt.SpellCheckingType = SpellChecking ? UITextSpellCheckingType.Default : UITextSpellCheckingType.No;
            txt.AutocapitalizationType = SpellChecking ? txt.AutocapitalizationType : UITextAutocapitalizationType.None;
            return txt;
        }
    }
}

