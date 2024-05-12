using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using FunkinSharp.Game.Core.Sparrow;
using FunkinSharp.Game.Core.Utils;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;

namespace FunkinSharp.Game.Core
{
    // TODO: Add proper caching
    public static class Paths
    {
        // Cache stuff, tries to dispose most of the stuff when it cans
        private static Dictionary<string, Texture> keyedTextures = new Dictionary<string, Texture>();
        private static Dictionary<string, SparrowAtlas> keyedAtlases = new Dictionary<string, SparrowAtlas>();
        private static Dictionary<string, Track> keyedTracks = new Dictionary<string, Track>();

        private static NativeStorage cwdStorage; // Current Working Directory Storage, refers to the current path where the executable is
        private static NativeStorage assetsStorage; // Refers to the asset folder created

        // Needed game instances to be able to build usable objects in game

        private static GameHost game_host; // The current game host
        private static IRenderer renderer => game_host?.Renderer; // To create Textures from native storages
        private static AudioManager audio_manager; // To be able to add audios from native storage and work properly

        // Game Resource stores
        private static ResourceStore<byte[]> dllResources; // DLL Resources, we only need this one since we are explicitly making the paths by ourselves

        public static void Initialize(GameHost host, AudioManager audioManager, ResourceStore<byte[]> resources)
        {
            game_host = host;
            audio_manager = audioManager;
            dllResources = resources;

            cwdStorage = new NativeStorage(Path.GetDirectoryName(host.FullPath), host);
            assetsStorage = (NativeStorage)cwdStorage.GetStorageForDirectory("assets");
            createPaths();
        }

        // Creates the same folder tree schema from FNF 0.3 or standard HaxeFlixel
        private static void createPaths()
        {
            // We need assets storage created before this (?)

        }

        // We will always look for the asset inside the game resources first
        // or by checking the path, knowing we gotta either use game resources or native storage
        // since most of the paths provided in game doesnt start with assets or a full path

        // TODO: Be able to pass more arguments for the texture generation
        public static SparrowAtlas GetSparrow(string path)
        {
            // Save the check before appending the missing extension (?)
            string assetPath = cwdStorage.GetFullPath("assets");
            bool isFromNative = path.StartsWith(assetPath);

            if (!path.EndsWith(".xml"))
                path += ".xml";

            Cache(path, out SparrowAtlas catlas);
            if (catlas != null)
                return catlas;

            XmlReader xmlReader;

            // Provided path is from NativeStorage
            if (isFromNative)
            {
                // TODO: Proper fallbacking?
                xmlReader = XmlReader.Create(assetsStorage.GetStream(path));
            }
            else // Provided path is from DLL Resources
            {
                xmlReader = XmlReader.Create(dllResources.GetStream(path));
            }

            SparrowAtlas atlas = AssetFactory.ParseSparrow(xmlReader);

            // overhaul this shi bru
            Texture endTexture;

            string imagePath = isFromNative ?
                Path.Join(Path.GetDirectoryName(path) ?? "", atlas.TextureName) :
                SanitizeForResources($"{Path.GetDirectoryName(path) ?? ""}/{atlas.TextureName}");

            Cache(atlas.TextureName, out Texture ctexture);
            if (ctexture != null)
                endTexture = ctexture;
            else
                endTexture = Cache(atlas.TextureName, GetTexture(imagePath));

            atlas.BuildFrames(endTexture, WrapMode.ClampToEdge, WrapMode.ClampToEdge);
            Cache(path, atlas);
            return atlas;
        }

        public static Track GetTrack(string path)
        {
            throw new NotImplementedException();
        }

        // TODO: improve this since a full proper path is expected
        public static Texture GetTexture(string path)
        {
            string assetPath = cwdStorage.GetFullPath("assets");
            bool isFromNative = path.StartsWith(assetPath);

            return AssetFactory.CreateTexture(renderer, (isFromNative) ? assetsStorage.GetStream(path) : dllResources.GetStream(path), TextureFilteringMode.Linear, WrapMode.ClampToEdge, WrapMode.ClampToEdge, true);
        }

        // replaces '\' and the directory separator character with '/'
        public static string SanitizeForResources(string path) =>
            path.Replace('\\', '/').Replace(Path.DirectorySeparatorChar, '/');

        // Be able to add cache externally
        public static Texture Cache(string key, Texture value) => keyedTextures[key] = value;
        public static SparrowAtlas Cache(string key, SparrowAtlas value) => keyedAtlases[key] = value;
        public static Track Cache(string key, Track value) => keyedTracks[key] = value;

        // Be able to access it externally
        public static void Cache(string key, out Texture cached) => cached = keyedTextures.ContainsKey(key) ? keyedTextures[key] : null;
        public static void Cache(string key, out SparrowAtlas cached) => cached = keyedAtlases.ContainsKey(key) ? keyedAtlases[key] : null;
        public static void Cache(string key, out Track cached) => cached = keyedTracks.ContainsKey(key) ? keyedTracks[key] : null;

    }
}
