namespace FunkinSharp.Game.Core
{
    public readonly struct AnimationFrame
    {
        public readonly int StartFrame;
        public readonly int EndFrame;
        public readonly int Frames;
        public readonly int FrameRate;

        public readonly int[] Indices;

        public AnimationFrame(int startFrame, int endFrame, int frames, int frameRate = 24)
        {
            StartFrame = startFrame;
            EndFrame = endFrame;
            Frames = frames;
            FrameRate = frameRate;
        }

        public AnimationFrame(int[] indices, int frameRate = 24)
        {
            Indices = indices;
            Frames = indices.Length - 1;
            FrameRate = frameRate;
        }
    }
}
