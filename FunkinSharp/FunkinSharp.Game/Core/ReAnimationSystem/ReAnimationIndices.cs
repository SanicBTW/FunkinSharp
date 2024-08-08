using System.Collections.Generic;

namespace FunkinSharp.Game.Core.ReAnimationSystem
{
    // extends reanimation so it can be used without any other compatibilty layer
    // using old FrameAnimatedSprite indices code
    public partial class ReAnimationIndices : ReAnimation
    {
        public ReAnimationIndices(ReAnimatedSprite parent, string animation) : base(parent, animation) { }

        public void AddByIndices(string name, string prefix, int[] indices, string postfix, double frameDuration = DEFAULT_FRAMERATE, bool loop = true,
            bool flipX = false, bool flipY = false)
        {
            if (Frames.Count > 0)
            {
                List<int> frameIndices = [];
                pushIndicesHelper(frameIndices, prefix, indices, postfix);

                // Replace the existing animation with the indices one
                if (frameIndices.Count > 0)
                {
                    FrameRate = frameDuration;
                    Loop = loop;
                    FlipHorizontal = flipX;
                    FlipVertical = flipY;

                    List<int> oldFrames = Frames;
                    List<int> newFrames = [];

                    foreach (int frameIndex in frameIndices)
                    {
                        newFrames.Add(oldFrames[frameIndex]);
                    }

                    Frames = newFrames;
                    Controller.Animations[name] = this;
                }
            }
        }

        private int findSpriteFrame(string prefix, int index, string postfix)
        {
            var i = 0;
            foreach (int frame in Frames)
            {
                string name = Controller.Frames[frame].Name;
                if (name.StartsWith(prefix) && name.EndsWith(postfix))
                {
                    var endIndex = name.Length - postfix.Length;
                    if (int.TryParse(name[prefix.Length..endIndex], out int frameIndex))
                    {
                        if (frameIndex == index)
                            return i;
                    }
                }

                i++;
            }

            return -1;
        }

        private void pushIndicesHelper(in List<int> target, string prefix, int[] indices, string suffix)
        {
            foreach (int index in indices)
            {
                int indexToAdd = findSpriteFrame(prefix, index, suffix);
                if (indexToAdd != -1)
                    target.Add(indexToAdd);
            }
        }
    }
}
