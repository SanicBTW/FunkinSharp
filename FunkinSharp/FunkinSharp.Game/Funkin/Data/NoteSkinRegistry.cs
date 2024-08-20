using System.Collections.Generic;
using System.IO;
using System.Xml;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Funkin.Compat;
using Newtonsoft.Json;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osu.Framework.Platform;

namespace FunkinSharp.Game.Funkin.Data
{
    public static class NoteSkinRegistry
    {
        // TODO: Guard cases where the skin texture has a name that its already used, this conflicts with the cache
        private static NativeStorage skinsFolder;

        public static void Initialize(IStorageService storage)
        {
            // the IStorageService dependency (cached btw) is bound to the current working directory
            skinsFolder = (NativeStorage)storage.GetStorageForDirectory("noteskins");
            copyDefault();
        }

        public static List<string> Scan()
        {
            List<string> ret = [];

            List<string> skins = [..skinsFolder.GetDirectories(".")];

            foreach (string skin in skins)
            {
                NativeStorage skinStor = (NativeStorage)skinsFolder.GetStorageForDirectory(skin);
                if (skinStor.Exists($"{skin}.json"))
                    ret.Add(skin);
                else
                {
                    Logger.Log($"Couldn't add Note Skin {skin} because it's missing the JSON file.", LoggingTarget.Runtime, LogLevel.Debug);
                    continue;
                }
            }

            return ret;
        }

        // These functions should only be used if the Scan function returned the provided skin
        // TODO: Add a reference in the fe receptor data about the new sustain sheet texture name
        public static bool SupportsSustainSheet(string skin)
        {
            NativeStorage skinStorage = (NativeStorage)skinsFolder.GetStorageForDirectory(skin);
            return skinStorage.Exists($"{skin}_hold_assets.png");
        }

        public static FEReceptorData GetSkinData(string skin)
        {
            NativeStorage skinStorage = (NativeStorage)skinsFolder.GetStorageForDirectory(skin);
            Stream stream = skinStorage.GetStream($"{skin}.json");
            using StreamReader reader = new StreamReader(stream);
            return JsonConvert.DeserializeObject<FEReceptorData>(reader.ReadToEnd());
        }

        public static XmlReader GetSkinSpritesheet(string skin)
        {
            NativeStorage skinStorage = (NativeStorage)skinsFolder.GetStorageForDirectory(skin);
            FEReceptorData data = GetSkinData(skin);
            return XmlReader.Create(skinStorage.GetStream($"{data.Texture}.xml"));
        }

        // from sustain sheet will be only be passed as true whenever the skin supports it
        public static Texture GetSkinTexture(string skin, bool fromSustainSheet = false)
        {
            NativeStorage skinStorage = (NativeStorage)skinsFolder.GetStorageForDirectory(skin);
            FEReceptorData data = GetSkinData(skin);
            string targetTexture = $"{data.Texture}.png";
            if (fromSustainSheet)
                targetTexture = $"{skin}_hold_assets.png";

            Paths.Cache(targetTexture, out Texture ctexture);
            if (ctexture != null)
                return ctexture;

            Texture texture = Paths.CreateTextureFromStream(skinStorage.GetStream(targetTexture), false, WrapMode.ClampToEdge, WrapMode.ClampToEdge);
            Paths.Cache(targetTexture, texture);
            return texture;
        }

        // this copies over the dll resources from the specified path into the desired native storage directory
        // only ran at initialization
        private static void copyDefault()
        {
            string targetSkin = "funkin";

            if (!skinsFolder.ExistsDirectory(targetSkin))
            {
                NativeStorage skinStorage = (NativeStorage)skinsFolder.GetStorageForDirectory(targetSkin);

                string dllPath = $"NoteTypes/{targetSkin}/";
                foreach (string res in Paths.AvailableResources)
                {
                    if (res.StartsWith(dllPath))
                    {
                        Stream content = Paths.GetStream(res);
                        string fileName = res.Replace(dllPath, "");

                        // since the way to check if the skin supports the new sustain sheet is like "<skin>_hold_assets" and to avoid mismatching cache (since i dont do proper caching)
                        // we replace the file name to meet the criteria 
                        if (fileName.ToLower() == "note_hold_assets.png")
                            fileName = fileName.Replace("NOTE", "funkin");

                        Stream file = skinStorage.CreateFileSafely(fileName);
                        content.CopyTo(file);
                        file.Close();
                    }
                    else
                        continue;
                }
            }
        }
    }
}
