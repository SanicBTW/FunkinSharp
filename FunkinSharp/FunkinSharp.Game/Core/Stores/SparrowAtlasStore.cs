using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using FunkinSharp.Game.Core.ReAnimationSystem;
using FunkinSharp.Game.Core.Sparrow;
using FunkinSharp.Game.Core.Utils;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;

// https://github.com/GetFunkin/Funkin.NET/blob/master-GONE/Funkin.NET.Intermediary/ResourceStores/SparrowAtlasStore.cs
namespace FunkinSharp.Game.Core.Stores
{
    /// <summary>
    ///     ResourceStore designed for caching Textures loaded from a SparrowAtlas.
    /// </summary>
    public class SparrowAtlasStore : ResourceStore<byte[]>
    {
        protected readonly IRenderer Renderer;

        public SparrowAtlasStore(IResourceStore<byte[]> store = null, IRenderer renderer = null) : base(store)
        {
            Renderer = renderer;
            AddExtension("xml");
        }

        public SparrowAtlas GetSparrow(string name,
            bool bypassTextureUploadQueueing = false,
            WrapMode hWrap = WrapMode.ClampToEdge,
            WrapMode vWrap = WrapMode.ClampToEdge,
            TextureFilteringMode fMode = TextureFilteringMode.Linear)
        {
            if (!name.EndsWith(".xml"))
                name += ".xml";

            Paths.Cache(name, out SparrowAtlas catlas);
            if (catlas != null)
                return catlas;

            using XmlReader xmlReader = XmlReader.Create(GetStream(name));
            SparrowAtlas atlas = AssetFactory.ParseSparrowLegacy(xmlReader);

            Texture endTexture;
            string imagePath = Paths.SanitizeForResources($"{Path.GetDirectoryName(name) ?? ""}/{atlas.TextureName}");
            if (!bypassTextureUploadQueueing)
            {
                Paths.Cache(atlas.TextureName, out Texture ctexture);
                if (ctexture != null)
                    endTexture = ctexture;
                else
                    endTexture = Paths.Cache(atlas.TextureName, Paths.GetTexture(imagePath, false));
            }
            else
                endTexture = AssetFactory.CreateTexture(Renderer, GetStream(imagePath), fMode, hWrap, vWrap);

            endTexture.BypassTextureUploadQueueing = bypassTextureUploadQueueing;
            atlas.BuildFrames(endTexture, hWrap, vWrap);
            Paths.Cache(name, atlas);
            return atlas;
        }

        public void GetSparrowNew(in ReAnimatedSprite controller, string file)
        {
            if (!file.EndsWith(".xml"))
                file += ".xml";

            using XmlReader xmlReader = XmlReader.Create(GetStream(file));
            AssetFactory.ParseSparrowNew(controller, Path.GetDirectoryName(file), xmlReader);
        }
    }
}
