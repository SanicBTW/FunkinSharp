using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;

namespace FunkinSharp.Game.Core.ReAnimationSystem
{
    // Simple Frame implementation
    // I kinda hate not allocating the size and offset vectors, but I wanted to unify them since they are gathered from the size rectangle (see AssetFactory)
    public class ReAnimationFrame
    {
        public string Name { get; set; }
        public Texture TextureFrame { get; set; }
        public RectangleF Frame { get; set; }
        public RectangleF Rect { get; set; }
        public bool Rotated { get; set; }
    }
}
