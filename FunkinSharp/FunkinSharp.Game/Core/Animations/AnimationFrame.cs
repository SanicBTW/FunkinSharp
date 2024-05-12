namespace FunkinSharp.Game.Core.Animations
{
    public readonly struct AnimationFrame
    {
        public readonly int StartFrame;
        public readonly int EndFrame;
        public readonly int Frames;
        public readonly int FrameRate;

        public readonly int[] Indices;

        public readonly bool Loop;

        public AnimationFrame(int startFrame, int endFrame, int frames, int frameRate = 24, bool loop = false)
        {
            StartFrame = startFrame;
            EndFrame = endFrame;
            Frames = frames;
            FrameRate = frameRate;
            Loop = loop;
        }

        public AnimationFrame(int[] indices, int frameRate = 24, bool loop = false)
        {
            Indices = indices;
            Frames = indices.Length - 1;
            FrameRate = frameRate;
            Loop = loop;
        }
    }
}
