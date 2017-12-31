using System.Linq;
using UIKit;
using CodeHub.Core.Data;
using System.Threading.Tasks;
using CodeHub.iOS.DialogElements;
using System.Reactive.Subjects;
using System;
using System.Reactive.Linq;
using Foundation;
using CodeHub.iOS.Utilities;
using System.Net;

namespace CodeHub.iOS.ViewControllers.Repositories
{
    public class LanguagesViewController : DialogViewController
    {
        private readonly ISubject<Language> _languageSubject = new Subject<Language>();

        public IObservable<Language> Language
        {
            get { return _languageSubject.AsObservable(); }
        }

        public Language SelectedLanguage { get; set; }

        public LanguagesViewController()
            : base(UITableViewStyle.Plain)
        {
            Title = "Languages";
            EnableSearch = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Load().ToBackground();
        }

        private async Task Load()
        {
            NetworkActivity.PushNetworkActive();

            try
            {
                await LoadLanguages();
            }
            finally
            {
                NetworkActivity.PopNetworkActive();
            }
        }

        private async Task LoadLanguages()
        {
            var lRepo = new LanguageRepository();
            var langs = await lRepo.GetLanguages();

            var sec = new Section();

            langs.Insert(0, new Language("All Languages", null));
            sec.AddAll(langs.Select(x =>
            {
                var el = new StringElement(x.Name) { Accessory = UITableViewCellAccessory.None };
                el.Clicked.Subscribe(_ => _languageSubject.OnNext(x));
                return el;
            }));

            Root.Reset(sec);

            if (SelectedLanguage != null)
            {
                var el = sec.Elements.OfType<StringElement>().FirstOrDefault(x => string.Equals(x.Caption, SelectedLanguage.Name));
                if (el != null)
                    el.Accessory = UITableViewCellAccessory.Checkmark;

                var indexPath = el?.IndexPath;
                if (indexPath != null)
                    TableView.ScrollToRow(indexPath, UITableViewScrollPosition.Middle, false);
            }
        }
    }
}