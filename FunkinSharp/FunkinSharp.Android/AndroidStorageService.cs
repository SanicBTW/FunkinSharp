using System.Collections.Generic;
using System.IO;
using FunkinSharp.Game.Core;
using osu.Framework.Platform;
using Environment = Android.OS.Environment;

namespace FunkinSharp.Android
{
    public class AndroidStorageService : IStorageService
    {
        private NativeStorage storage;

        public AndroidStorageService(MainActivity gameActivity, GameHost host)
        {
            // too bad so sad, i cant change the directory so fuck it (i might add an option to accept a directory on settings or sum so android isnt pissed off)
            string dir = Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDocuments).AbsolutePath;
            storage = new NativeStorage(Path.Join(dir, "FunkinSharp"), host);
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
