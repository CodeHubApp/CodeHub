using System;
using System.Collections.Generic;
using System.IO;
using CodeHub.Core.Services;

namespace CodeHub.Android.Services
{
    class FilesystemService : IFilesystemService
    {
        public Stream CreateTempFile(out string path, string name = null)
        {
            path = name == null ? Path.GetTempFileName() : Path.Combine(Path.GetTempPath(), name);
            return new FileStream(path, FileMode.Create, FileAccess.Write);
        }

        public string DocumentDirectory
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); }
        }

        public IEnumerable<string> GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }
    }
}