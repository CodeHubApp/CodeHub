using System.IO;
using System.Collections.Generic;

namespace CodeHub.Core.Services
{
    public interface IFilesystemService
    {
        string DocumentDirectory { get; }

        Stream CreateTempFile(out string path, string name = null);

        IEnumerable<string> GetFiles(string path);
    }
}
