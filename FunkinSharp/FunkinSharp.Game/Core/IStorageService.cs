using System.Collections.Generic;
using System.IO;
using osu.Framework.Platform;

namespace FunkinSharp.Game.Core
{
    // An attempt on joining file support without too much fuss across platforms (Mobile/Desktop) while being a wrapper of NativeStorage
    // Please look at the following files for the respective implementation of the Storage Service on the platform
    // - FunkinSharp.Android/AndroidStorageService for the Android implementation
    // - FunkinSharp.Desktop/DesktopStorageService for the Windows/Linux implementation
    /// <summary>
    /// Wrapper for <see cref="Storage"/> and <seealso cref="NativeStorage"/> on most of the platforms while keeping the usage clean.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Returns the path being used in THIS <see cref="IStorageService"/> and <seealso cref="NativeStorage"/>.
        /// </summary>
        string BasePath { get; } // GetFullPath(" ")

        /// <summary>
        /// Get a usable filesystem path for the provided incomplete path.
        /// </summary>
        /// <param name="path">An incomplete path, usually provided as user input.</param>
        /// <param name="createIfNotExisting">Create the path if it doesn't already exist.</param>
        /// <returns>A usable filesystem path.</returns>
        string GetFullPath(string path, bool createIfNotExisting = false); // NativeStorage.GetFullPath(path, createIfNotExisting)

        /// <summary>
        /// Check whether a file exists at the specified path.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>Whether a file exists.</returns>
        bool Exists(string path); // NativeStorage.Exists(path)

        /// <summary>
        /// Check whether a directory exists at the specified path.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>Whether a directory exists.</returns>
        bool ExistsDirectory(string path); // NativeStorage.ExistsDirectory(path)

        /// <summary>
        /// Delete a directory and all its contents recursively.
        /// </summary>
        /// <param name="path">The path of the directory to delete.</param>
        void DeleteDirectory(string path); // NativeStorage.DeleteDirectory(path)

        /// <summary>
        /// Delete a file.
        /// </summary>
        /// <param name="path">The path of the file to delete.</param>
        void Delete(string path); // NativeStorage.Delete(path)

        /// <summary>
        /// Retrieve a list of directories at the specified path.
        /// </summary>
        /// <param name="path">The path to list.</param>
        /// <returns>A list of directories in the path, relative to the path of this storage.</returns>
        IEnumerable<string> GetDirectories(string path); // NativeStorage.GetDirectories(path)

        /// <summary>
        /// Retrieve a list of files at the specified path.
        /// </summary>
        /// <param name="path">The path to list.</param>
        /// <param name="pattern">An optional search pattern. Accepts "*" wildcard.</param>
        /// <returns>A list of files in the path, relative to the path of this storage.</returns>
        IEnumerable<string> GetFiles(string path, string pattern = "*"); // NativeStorage.GetFiles(path, pattern)

        /// <summary>
        /// Retrieve a <see cref="Storage"/> for a contained directory.
        /// Creates the path if not existing.
        /// </summary>
        /// <param name="path">The subdirectory to use as a root.</param>
        /// <returns>A more specific storage.</returns>
        Storage GetStorageForDirectory(string path); // NativeStorage.GetStorageForDirectory(path)

        /// <summary>
        /// Move a file from one location to another. File must exist. Destination will be overwritten if exists.
        /// </summary>
        /// <param name="from">The file path to move.</param>
        /// <param name="to">The destination path.</param>
        void Move(string from, string to); // NativeStorage.Move(from, to)

        /// <summary>
        /// Create a new file on disk, using a temporary file to write to before moving to the final location to ensure a half-written file cannot exist at the specified location.
        /// </summary>
        /// <remarks>
        /// If the target file path already exists, it will be deleted before attempting to write a new version.
        /// </remarks>
        /// <param name="path">The path of the file to create or overwrite.</param>
        /// <returns>A stream associated with the requested path. Will only exist at the specified location after the stream is disposed.</returns>
        Stream CreateFileSafely(string path); // NativeStorage.CreateFileSafely(path)

        /// <summary>
        /// Retrieve a stream from an underlying file inside this storage.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="access">The access requirements.</param>
        /// <param name="mode">The mode in which the file should be opened.</param>
        /// <returns>A stream associated with the requested path.</returns>
        Stream GetStream(string path, FileAccess access = FileAccess.Read, FileMode mode = FileMode.OpenOrCreate); // NativeStorage.GetStream(path, access, mode)

        /// <summary>
        /// Requests that a file be opened externally with an associated application, if available.
        /// </summary>
        /// <param name="filename">The relative path to the file which should be opened.</param>
        /// <returns>Whether the file was successfully opened.</returns>
        bool OpenFileExternally(string filename); // NativeStorage.OpenFileExternally(filename)

        /// <summary>
        /// Opens a native file browser window to the root path of this storage.
        /// </summary>
        /// <returns>Whether the storage was successfully presented.</returns>
        bool PresentExternally() => OpenFileExternally(string.Empty); // NativeStorage.OpenFileExternally(filename), although already referenced here

        /// <summary>
        /// Requests to present a file externally in the platform's native file browser.
        /// </summary>
        /// <remarks>
        /// This will open the parent folder and, (if available) highlight the file.
        /// </remarks>
        /// <param name="filename">Relative path to the file.</param>
        /// <returns>Whether the file was successfully presented.</returns>
        bool PresentFileExternally(string filename); // NativeStorage.PresentFileExternally(filename)
    }
}
