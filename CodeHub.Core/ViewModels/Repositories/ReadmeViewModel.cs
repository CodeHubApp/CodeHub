using System;
using System.Text;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class ReadmeViewModel : BaseViewModel
    {
        private string _data;
        private string _path;

        public string Username { get; private set; }

        public string Repository { get; private set; }

        public string Data
        {
            get { return _data; }
            set { _data = value; RaisePropertyChanged(() => Data); }
        }

        public string Path
        {
            get { return _path; }
            set { _path = value; RaisePropertyChanged(() => Path); }
        }

        private void Load()
        {
            var wiki = Application.Client.Execute(Application.Client.Users[Username].Repositories[Repository].GetReadme()).Data;
            var d = Encoding.UTF8.GetString(Convert.FromBase64String(wiki.Content));
            Data = Application.Client.Markdown.GetMarkdown(d);
            Path = CreateHtmlFile(Data);
        }

        private string CreateHtmlFile(string data)
        {
            //Generate the markup
            var markup = new StringBuilder();
            markup.Append("<html><head>");
            markup.Append("<meta name=\"viewport\" content=\"width=device-width; initial-scale=1.0; maximum-scale=1.0; user-scalable=0\"/>");
            markup.Append("<title>Readme");
            markup.Append("</title></head><body>");
            markup.Append(data);
            markup.Append("</body></html>");

            var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetTempFileName() + ".html");
            System.IO.File.WriteAllText(tmp, markup.ToString(), System.Text.Encoding.UTF8);
            return tmp;
        }

        public void Init(NavObject navObject)
        {
            Username = navObject.Username;
            Repository = navObject.Repository;
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}
