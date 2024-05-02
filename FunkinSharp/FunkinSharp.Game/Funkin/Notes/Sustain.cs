using FunkinSharp.Game.Core;
using osu.Framework.Graphics;

namespace FunkinSharp.Game.Funkin.Notes
{
    // TODO: Proper End positioning or alpha stuff (if positioning doesnt end up being a thing)
    public partial class Sustain : ClippedContainer
    {
        public readonly Note Head; // Holds the useful stuff

        // These sprites are added to "this" container (the clip container)
        public SustainSprite Body { get; private set; }
        public SustainEnd End { get; private set; }

        public float TargetHeight; // This fixes the sustain appearing in the middle screen while spawning it in game

        public Sustain(Note head)
        {
            Head = head;
            head.BoundToSustain = true;

            Add(Body = new SustainSprite(head));
            Body.OnLoadComplete += body_OnLoadComplete;

            Alpha = 0.8f;
            RelativeSizeAxes = Axes.None; // I went insane for about an hour and I only had to do this bro
        }

        private void body_OnLoadComplete(Drawable obj)
        {
            // Now it works as expected :sob:
            Width = Body.CurrentFrame.DisplayWidth * Head.ReceptorData.Size;

            Anchor = Origin = Body.Anchor;

            // Create the end when the body is done
            Add(End = new SustainEnd(Head));
        }

        protected override void Update()
        {
            // This is to set the Y position of the end sprite, currently the body is halfway inside the end but isnt too visible
            // TODO: Add checks to see if the stuff is alive or not
            if (Head.IsLoaded && Body.IsLoaded && End.IsLoaded)
            {
                float targetHeight = Height - (End.CurrentFrame.DisplayHeight + End.Y);

                Body.Height = (targetHeight / Height);

                End.Y = ((Body.Height / targetHeight) * Head.ReceptorData.Size) +
                    ((End.CurrentFrame.DisplayHeight * Head.ReceptorData.Size) * Head.ReceptorData.Size) -
                    (End.CurrentFrame.DisplayHeight * Head.ReceptorData.Size);

                Y = Head.Y + Head.AnchorPosition.Y;
                Height = TargetHeight;
            }

            base.Update();
        }
    }
}
