using System.Collections.Generic;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;

namespace FunkinSharp.Game.Core.Sparrow
{
    // This code is some legacy shit I cooked back on the prototype version, I don't really know how to rewrite it lol
    public class SparrowAtlas
    {
        public readonly string TextureName;
        public Dictionary<string, AnimationFrame> Animations { get; private set; } = []; // List of playable animations
        public List<Texture> Frames { get; private set; } = []; // Publicly exposed frames so they are ready upon parsing

        private List<RectangleF> regions = []; // Used for TextureRegions to get the frame of the SparrowAtlas

        public SparrowAtlas(string textureName)
        {
            TextureName = textureName;
        }

        public void SetFrame(string name, AnimationFrame frame)
        {
            Animations[name] = frame;
        }

        public void AddRegion(RectangleF rect)
        {
            regions.Add(rect);
        }

        public void BuildFrames(in Texture texture)
        {
            if (regions.Count <= 0) return; // Already parsed, frames are available

            foreach (RectangleF region in regions)
            {
                TextureRegion frame = new TextureRegion(texture, region, WrapMode.Repeat, WrapMode.Repeat);
                Frames.Add(frame);
            }
            regions.Clear();
        }
    }
}
