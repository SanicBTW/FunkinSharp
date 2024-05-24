using FunkinSharp.Game.Core.Containers;
using FunkinSharp.Game.Funkin.Song;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace FunkinSharp.Game.Funkin.Notes
{
    // TODO: End sprite is somewhat offsetted or not properly scaled
    public partial class Sustain : ClippedContainer
    {
        public readonly Note Head; // Holds the useful stuff
        // These sprites are added to "this" container (the clip container)
        private BufferedContainer<SustainSprite> bufferedBody; // This is added but the body is accessed through its own variable
        public SustainSprite Body { get; private set; } // Gets added to buffered container
        public SustainEnd End { get; private set; }

        public float MaxHeight; // The maximum height this sustain can reach, used to clamp the TargetHeight with this as the max
        public float TargetHeight; // This fixes the sustain appearing in the middle screen while spawning it in game

        public readonly BindableFloat Speed = new BindableFloat(); // Bound to the strumline when pushed

        // Sustain length
        private float susLength = 0;
        public float FullLength;
        public float Length
        {
            get => susLength;
            set
            {
                if (value < 0.0f)
                    value = 0.0f;

                if (susLength == value) return;

                TargetHeight = SustainHeight(value, Speed.Value);
                susLength = value;
            }
        } 

        public bool Holding = false; // Is the sustain currently being pressed?
        public float Holded = 0; // Time that the sustain has been pressed (StepCrochet)

        public bool Missed = false;
        public bool Hit = false;

        public float StrumTime => Head?.StrumTime ?? 0; // Uses the parent strum time since it was generated from that

        // Save a reference to THIS strumline sustain clipper to be able to modify it later on
        public ClippedContainer<Sustain> Clipper;

        public Sustain(Note head)
        {
            // Recalculate the height
            Speed.BindValueChanged((v) =>
            {
                float prev = susLength;
                susLength = 0;
                Length = prev;
            });

            // This bad boy blits to the screen but it somehow still works fine & fast
            bufferedBody = new BufferedContainer<SustainSprite>(null, true, true)
            {
                RelativeSizeAxes = Axes.X
            };

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
            Add(bufferedBody);
            bufferedBody.Child = Body = new SustainSprite(Head); // Since the body is already added on the buffered container, theres no need to re-add it to this container
            Body.OnLoadComplete += body_OnLoadComplete;
        }

        private void body_OnLoadComplete(Drawable obj)
        {
            // https://github.com/ppy/osu-framework/discussions/6278#discussioncomment-9373679
            bufferedBody.FrameBufferScale = new Vector2(bufferedBody.FrameBufferScale.X, 0);

            // Now it works as expected :sob:
            Width = Body.CurrentFrame.DisplayWidth * Head.Scale.X;

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

                Height = float.Clamp(TargetHeight, 0, MaxHeight);

                // the sustain body automatically resizes to fit the buffered container
                bufferedBody.Height = (Height - End.CurrentFrame.DisplayHeight);

                Y = (Head.Y + Head.AnchorPosition.Y);

                if (Missed && Alpha != 0.3f)
                {
                    Alpha = 0.3f;
                    Head.Alpha = 0.3f;
                }
            }

            base.Update();
        }

        // https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/play/notes/SustainTrail.hx#L148
        // 0.45 (Pixels per ms constant) x 1.2 (magic number idk i got this out of nowhere) is to make the sustain fully match the timing and holding (in most cases)
        public static float SustainHeight(float susLength, float scrollSpeed) => (susLength * (SongConstants.PIXELS_PER_MS * 1.2f) * scrollSpeed);

    }
}
