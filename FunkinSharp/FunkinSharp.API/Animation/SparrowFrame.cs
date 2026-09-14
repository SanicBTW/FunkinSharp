using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Textures;

namespace FunkinSharp.API.Animation;

// theres no need for frame to know if its rotated or not, since it already gets applied on parsing
public sealed class SparrowFrame
{
    public required string Name { get; init; }
    public required Texture Atlas { get; init; }
    public required RectangleF Region { get; init; } // regio n inside the atlas
    public required RectangleF SourceRect { get; init; } // display dimensions
}
