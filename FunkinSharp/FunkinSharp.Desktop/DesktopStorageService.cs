using System.Collections.Generic;
using System.IO;
using FunkinSharp.Game.Core;
using osu.Framework.Platform;

namespace FunkinSharp.Desktop
{
    public class DesktopStorageService : IStorageService
    {
        private GameHost host;
        private NativeStorage storage;

        public DesktopStorageService(GameHost host)
        {
            this.host = host;

            storage = new NativeStorage(Path.GetDirectoryName(host.FullPath), host);
        }

        public string BasePath => storage.GetFullPath(" ");

        public string GetFullPath(string path, bool createIfNotExisting = false) => storage.GetFullPath(path, createIfNotExisting);

        public bool Exists(string path) => storage.Exists(path);

        public bool ExistsDirectory(string path) => storage.ExistsDirectory(path);

        public void DeleteDirectory(string path) => storage.DeleteDirectory(path);

        public void Delete(string path) => storage.Delete(path);

        public IEnumerable<string> GetDirectories(string path) => storage.GetDirectories(path);

        public IEnumerable<string> GetFiles(string path, string pattern = "*") => storage.GetFiles(path, pattern);

        public Storage GetStorageForDirectory(string path) => storage.GetStorageForDirectory(path);

        public void Move(string from, string to) => storage.Move(from, to);

        public Stream CreateFileSafely(string path) => storage.CreateFileSafely(path);

        public Stream GetStream(string path, FileAccess access = FileAccess.Read, FileMode mode = FileMode.OpenOrCreate) => storage.GetStream(path, access, mode);

        public bool OpenFileExternally(string filename) => storage.OpenFileExternally(filename);

        public bool PresentFileExternally(string filename) => storage.PresentFileExternally(filename);
    }
}
