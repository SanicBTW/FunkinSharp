using System.Collections.Generic;
using System.IO;
using System.Xml;
using FunkinSharp.Game.Core.Sparrow;
using osu.Framework.Graphics.Primitives;
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
        protected readonly Dictionary<string, Texture> SparrowAtlases = [];
        protected readonly Dictionary<string, SparrowAtlas> CachedAtlases = [];

        protected readonly IRenderer Renderer;

        public SparrowAtlasStore(IResourceStore<byte[]> store = null, IRenderer renderer = null) : base(store)
        {
            Renderer = renderer;
            AddExtension("xml");
        }

        public SparrowAtlas GetSparrow(string name, bool bypassTextureUploadQueueing = false)
        {
            if (!name.EndsWith(".xml"))
                name += ".xml";

            if (CachedAtlases.ContainsKey(name))
                return CachedAtlases[name];

            SparrowAtlas atlas = null;
            using XmlReader xmlReader = XmlReader.Create(GetStream(name));

            bool start = true;
            string lastAnim = "";
            int i = 0, frames = 0;
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType is XmlNodeType.XmlDeclaration
                    or XmlNodeType.Comment
                    or XmlNodeType.Whitespace
                    or XmlNodeType.EndElement)
                    continue;

                if (xmlReader.NodeType is XmlNodeType.EndElement)
                    break;

                if (xmlReader.NodeType == XmlNodeType.Element && atlas == null)
                {
                    atlas = new SparrowAtlas(xmlReader.GetAttribute("imagePath"));
                    continue;
                }

                // Required values
                string animName = xmlReader.GetAttribute("name")[..^4].Trim(); // Aint no way I forgot to trim

                // properly parsing da animation name, this is just stupid but needed :sob:
                if (animName.EndsWith("1")) // it means theres 5 frame numbers
                    animName = animName[..^1].Trim();
                if (animName.Contains("instance"))
                    animName = animName.Replace("instance", "").Trim();

                int x = int.Parse(xmlReader.GetAttribute("x")!);
                int y = int.Parse(xmlReader.GetAttribute("y")!);
                int width = int.Parse(xmlReader.GetAttribute("width")!);
                int height = int.Parse(xmlReader.GetAttribute("height")!);

                if (start)
                {
                    lastAnim = animName;
                    start = false;
                }

                if (lastAnim != animName)
                {
                    atlas.SetFrame(lastAnim, new AnimationFrame(i - frames, i - 1, frames));
                    lastAnim = animName;
                    frames = 0;
                }

                atlas.AddRegion(new RectangleF(x, y, width, height));
                i++;
                frames++;
            }

            // Set the frame for the last animation
            // - Theres no way I didn't find this and had to use ChatGPT for this one :sob:
            atlas.SetFrame(lastAnim, new AnimationFrame(i - frames, i - 1, frames));

            Texture endTexture;
            string imagePath = Paths.SanitizeForResources($"{Path.GetDirectoryName(name) ?? ""}/{atlas.TextureName}");
            if (!bypassTextureUploadQueueing)
            {
                if (SparrowAtlases.ContainsKey(atlas.TextureName))
                    endTexture = SparrowAtlases[atlas.TextureName];
                else
                    endTexture = SparrowAtlases[atlas.TextureName] = Texture.FromStream(Renderer, GetStream(imagePath));
            }
            else
                endTexture = Texture.FromStream(Renderer, GetStream(imagePath));

            endTexture.BypassTextureUploadQueueing = bypassTextureUploadQueueing;
            atlas.BuildFrames(endTexture);
            return atlas;
        }
    }
}
