using System.Collections.Generic;
using System.IO;
using System.Xml;
using FunkinSharp.Game.Core.Sparrow;
using FunkinSharp.Game.Core.Utils;
using FunkinSharp.Resources;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;

namespace FunkinSharp.Game.Core
{
    // Currently Paths are used to load assets whenever you want instead of waiting the load function to run
    // In a near future, paths will resolve paths for external assets or allow zips to be dropped to run mods n shi
    // TODO: Use the framework caching system alongside this one
    // TODO: Rewrite this for better support and proper cleaning and shit???
    public static class Paths
    {
        // Cache stuff, tries to dispose most of the stuff when its able to do it
        private static Dictionary<string, Texture> keyedTextures = [];
        private static Dictionary<string, SparrowAtlas> keyedAtlases = []; // the reason behind we dont clean sparrow atlases its because some sprites might still use them so to avoid empty shi we keepin all of em
        private static Dictionary<string, Track> keyedTracks = [];
        private static List<string> localKeyedAssets = [];
        private static List<string> persistentAssets = [];

        // Needed game instances to be able to build usable objects in runtime

        private static GameHost game_host; // The current game host
        private static IRenderer renderer => game_host?.Renderer; // To create Textures from native storages
        private static AudioManager audio_manager; // To be able to add audios from native storage and work properly
        private static TextureStore textureStore; // So it uses the capability of adding textures to the backing atlases to save up memory

        // Game Resource stores
        private static ResourceStore<byte[]> dllResources = new(); // DLL Resources, we only need this one since we are explicitly making the paths by ourselves
        public static List<string> AvailableResources = [];

        public static void Initialize(GameHost host, AudioManager audioManager, TextureStore textures, out DllResourceStore[] resources)
        {
            game_host = host;
            audio_manager = audioManager;
            textureStore = textures;

            // TODO: in a future version of FunkinSharp, it should look for any .Mod dll in the output folder and load it here hehe
            resources = new DllResourceStore[1];
            dllResources.AddStore(resources[0] = new DllResourceStore(FunkinSharpResources.ResourceAssembly));
            AvailableResources = [.. dllResources.GetAvailableResources()];
        }

        /*
         * Now it only focuses on DLL Resources, i'm currently thinking on some idea of adding a system
         * where Paths asks some system (Plugins or Events) to resolve the provided path for it
         * since the current target of the engine is to only provide the full rhythm game experience
         * modding may come on a later stage but since i'm currently unable to mess with paths correctly
         * im not going to add ANY support for external assets
         */

        // This should resolve any library/mod assets if possible
        public static Stream GetStream(string path)
        {
            if (!AvailableResources.Contains(path))
                throw new System.Exception($"Couldn't find the resource {path}");

            Stream stream = dllResources.GetStream(path);
            return stream ?? throw new System.Exception($"{path} returned a null stream");
        }

        public static SparrowAtlas GetSparrowLegacy(string path)
        {
            if (!path.EndsWith(".xml"))
                path += ".xml";

            Cache(path, out SparrowAtlas catlas);
            if (catlas != null)
                return catlas;

            using XmlReader xmlReader = XmlReader.Create(GetStream(path));
            SparrowAtlas atlas = AssetFactory.ParseSparrowLegacy(xmlReader);

            // rewrite this shi bru
            Texture endTexture;

            string imagePath = SanitizeForResources($"{Path.GetDirectoryName(path) ?? ""}/{atlas.TextureName}");

            Cache(atlas.TextureName, out Texture ctexture);
            if (ctexture != null)
                endTexture = ctexture;
            else
                endTexture = Cache(atlas.TextureName, GetTexture(imagePath, false));

            atlas.BuildFrames(endTexture, WrapMode.ClampToEdge, WrapMode.ClampToEdge);
            Cache(path, atlas);
            return atlas;
        }

        public static Track GetTrack(string path)
        {
            Cache(path, out Track ctrack);
            if (ctrack != null)
                return ctrack;

            Track newTrack = AssetFactory.CreateTrack(GetStream(path), path);
            Cache(path, newTrack);
            audio_manager.TrackMixer.Add(newTrack);
            audio_manager.AddItem(newTrack);
            return newTrack;
        }

        // Adds a track to the mixer
        public static Track AddTrack(Track track)
        {
            Cache(track.Name, out Track ctrack);
            if (ctrack != null)
                return ctrack;

            Cache(track.Name, track);
            audio_manager.TrackMixer.Add(track);
            audio_manager.AddItem(track);
            return track;
        }

        // using the texture store can provide the usage of the texture atlas but breaks some other stuff, it is recommended to keep this off for spritesheets
        public static Texture GetTexture(string path, bool useTextureStore = true,
            WrapMode hWrap = WrapMode.None, WrapMode vWrap = WrapMode.None, TextureFilteringMode filtering = TextureFilteringMode.Linear)
        {
            Cache(path, out Texture ctexture);
            if (ctexture != null)
                return ctexture;

            Texture newTexture = CreateTextureFromStream(GetStream(path), useTextureStore, hWrap, vWrap, filtering);
            Cache(path, newTexture);
            return newTexture;
        }

        public static Texture CreateTextureFromStream(Stream stream, bool useTextureStore = true,
            WrapMode hWrap = WrapMode.None, WrapMode vWrap = WrapMode.None, TextureFilteringMode filtering = TextureFilteringMode.Linear)
        {
            return (useTextureStore) ?
                AssetFactory.CreateTexture(textureStore, stream, hWrap, vWrap) :
                AssetFactory.CreateTexture(renderer, stream, filtering, hWrap, vWrap);
        }

        // Replaces '\' and the directory separator character with '/'
        public static string SanitizeForResources(string path) =>
            path.Replace('\\', '/').Replace(Path.DirectorySeparatorChar, '/');

        // Cleaning functions
        public static bool RemoveTexture(string key)
        {
            if (keyedTextures.TryGetValue(key, out Texture value))
            {
                value.Dispose();
                keyedTextures.Remove(key);
                return true;
            }

            return false;
        }

        public static bool RemoveTrack(string key)
        {
            if (keyedTracks.TryGetValue(key, out Track value))
            {
                value.Stop();
                value.Dispose();
                audio_manager.TrackMixer.Remove(value);
                keyedTracks.Remove(key);
                return true;
            }

            return false;
        }

        // Track cleaning should be handled in another way but I cant access the current used tracks so yeah
        // Most of these functions should be handled in another way too, they just copy paste bruh

        public static void ClearUnusedMemory()
        {
            foreach (var kv in keyedTracks)
            {
                if (!localKeyedAssets.Contains(kv.Key) && !persistentAssets.Contains(kv.Key))
                    RemoveTrack(kv.Key);
            }

            // Run the GC
            game_host.Collect();

            Logger.Log("Clear Unused Memory called", LoggingTarget.Runtime, LogLevel.Debug);
        }

        public static void ClearStoredMemory()
        {
            localKeyedAssets = [];

            // Run the GC
            game_host.Collect();

            Logger.Log("Clear Stored Memory called", LoggingTarget.Runtime, LogLevel.Debug);
        }

        // TODO: Better naming or join the functions, like string key, in T value, the in arg can be passed as a holder for retrieving the cache or saving it to the cache

        // Be able to add cache externally
        public static Texture Cache(string key, Texture value)
        {
            value.AssetName = key;
            localKeyedAssets.Add(key);
            return keyedTextures[key] = value;
        }

        public static SparrowAtlas Cache(string key, SparrowAtlas value) => keyedAtlases[key] = value;

        public static Track Cache(string key, Track value)
        {
            localKeyedAssets.Add(key);
            return keyedTracks[key] = value;
        }

        // Be able to access it externally
        public static void Cache(string key, out Texture cached) => cached = keyedTextures.TryGetValue(key, out Texture value) ? value : null;
        public static void Cache(string key, out SparrowAtlas cached) => cached = keyedAtlases.TryGetValue(key, out SparrowAtlas value) ? value : null;
        public static void Cache(string key, out Track cached) => cached = keyedTracks.TryGetValue(key, out Track value) ? value : null;

        public static bool Exists(string file) => AvailableResources.Contains(file);
    }
}
