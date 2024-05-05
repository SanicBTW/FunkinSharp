using FunkinSharp.Game.Core;
using osu.Framework.Graphics;

namespace FunkinSharp.Game.Funkin.Notes
{
    // TODO: Missing sustains when stopped grabbing (guitar sustains)
    public partial class Sustain : ClippedContainer
    {
        public readonly Note Head; // Holds the useful stuff
        // These sprites are added to "this" container (the clip container)
        public SustainSprite Body { get; private set; }
        public SustainEnd End { get; private set; }

        public float MaxHeight; // The maximum height this sustain can reach, used to clamp the TargetHeight with this as the max
        public float TargetHeight; // This fixes the sustain appearing in the middle screen while spawning it in game
        public float Length; // Sustain length

        public bool Holding = false; // Is the sustain currently being pressed?
        public float Holded = 0; // Time that the sustain has been pressed (StepCrochet)

        public Sustain(Note head)
        {
            Head = head;
            head.BoundToSustain = true;
            // Had to chain load events since for some reason now on TestSceneSustain it throws because of missing note cache (even if cached before)
            head.OnLoadComplete += head_OnLoadComplete;

            Alpha = 0.8f;
            RelativeSizeAxes = Axes.None; // I went insane for about an hour and I only had to do this bro
            Depth = 1;
        }

        private void head_OnLoadComplete(Drawable obj)
        {
            Add(Body = new SustainSprite(Head));
            Body.OnLoadComplete += body_OnLoadComplete;
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
            // TODO: Add checks to see if the stuff is alive or not (I dont think that is neccesary)
            if (Head.IsLoaded && Body.IsLoaded && End.IsLoaded)
            {
                if ((MaxHeight == 0 && Height != TargetHeight))
                    MaxHeight = TargetHeight;

                float clampedHeight = float.Clamp(TargetHeight, 0, MaxHeight);
                Height = clampedHeight;

                float bodyTarget = Height - End.CurrentFrame.DisplayHeight;
                Body.Height = (bodyTarget / Height);

                End.Y = ((Body.Height / 2) * Head.ReceptorData.Size) +
                    ((End.CurrentFrame.DisplayHeight * Head.ReceptorData.Size) * Head.ReceptorData.Size) -
                    (End.CurrentFrame.DisplayHeight * Head.ReceptorData.Size);

                float baseY = (Head.Y + Head.AnchorPosition.Y);
                if (Parent.GetType() == typeof(ClippedContainer)) // This is to check if the Sustain is inside a lane limiter 
                    Y = baseY - (Head.DrawHeight);
                else
                    Y = baseY;
            }

            base.Update();
        }
    }
}
