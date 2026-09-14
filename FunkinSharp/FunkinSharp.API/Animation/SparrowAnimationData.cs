namespace FunkinSharp.API.Animation;

public sealed class SparrowAnimationData
{
    public required string Name { get; init; }

    public required int[] Frames { get; init; }
    public double FrameRate { get; set; }

    public bool Loop { get; set; }
    public int LoopPoint { get; set; }

    public bool FlipX { get; set; }
    public bool FlipY { get; set; }
}
