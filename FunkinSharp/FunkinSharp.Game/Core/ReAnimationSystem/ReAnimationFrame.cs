using osu.Framework.Graphics.Primitives;
using osuTK;

namespace FunkinSharp.Game.Core.ReAnimationSystem
{
    // Simple Frame implementation
    public class ReAnimationFrame
    {
        public string Name { get; set; }
        public RectangleF Frame { get; set; }
        public Vector2 Offset { get; set; }
        public Vector2 SourceSize { get; set; }
        public bool Rotated { get; set; }
    }
}
