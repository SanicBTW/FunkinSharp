using System.Collections.Generic;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;

namespace FunkinSharp.Game.Core.Sparrow
{
    // This code is some legacy shit I cooked back on the prototype version, I don't really know how to rewrite it lol
    // I currently don't really like how this code looks like so maybe once I finish some other stuff I'll get to rewrite most of the animation controller and parsing
    public class SparrowAtlas
    {
        public readonly string TextureName;
        public Dictionary<string, AnimationFrame> Animations { get; private set; } = []; // List of playable animations
        public List<Texture> Frames { get; private set; } = []; // Publicly exposed frames so they are ready upon parsing
        public List<string> FrameNames { get; private set; } = []; // The animation names that were found on the atlas

        private List<RectangleF> regions = []; // Used for TextureRegions to get the frame of the SparrowAtlas

        public SparrowAtlas(string textureName)
        {
            TextureName = textureName;
        }

        public void SetFrame(string name, AnimationFrame frame) => Animations[name] = frame;

        public void AddRegion(RectangleF rect) => regions.Add(rect);

        public void BuildFrames(in Texture texture, WrapMode horizontalWrap, WrapMode verticalWrap)
        {
            if (regions.Count <= 0) return; // Already parsed, frames are available

            foreach (RectangleF region in regions)
            {
                TextureRegion frame = new TextureRegion(texture, region, horizontalWrap, verticalWrap);
                Frames.Add(frame);
            }
            regions.Clear();
        }
    }
}
