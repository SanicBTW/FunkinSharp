using FunkinSharp.Game.Core.Containers;
using FunkinSharp.Game.Funkin.Song;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace FunkinSharp.Game.Funkin.Notes
{
    // TODO: End sprite is somewhat offsetted or not properly scaled
    // TODO: better naming :fire:
    // TODO: Move to the new notestyle json
    public partial class Sustain : ClippedContainer
    {
        public readonly Note Head; // Holds the useful stuff
        // These sprites are added to "this" container (the clip container)
        public SustainSprite Body { get; private set; }
        public SustainEnd End { get; private set; }

        public float MaxHeight; // The maximum height this sustain can reach, used to clamp the TargetHeight with this as the max
        public BindableFloat TargetHeight = new(); // This fixes the sustain appearing in the middle screen while spawning it in game

        // Bound to the strumline when pushed
        // TODO: Look for another way of setting downscroll or a mult to set the scroll positioning for more dynamic shi
        public readonly BindableFloat Speed = new();
        public readonly BindableBool Downscroll = new();

        // Sustain length
        private float susLength = 0;
        public float FullLength; // used to know the real sustain length without any operation
        public float Length
        {
            get => susLength;
            set
            {
                if (value < 0.0f)
                    value = 0.0f;

                if (susLength == value) return;

                TargetHeight.Value = SustainHeight(value, Speed.Value);
                susLength = value;
            }
        }

        public bool Holding = false; // Is the sustain currently being pressed?
        public float Holded = 0; // Time that the sustain has been pressed, only gets used when the sustain needs to get resized upon missing

        public bool Missed = false;
        public bool Hit = false;

        public float StrumTime => Head?.StrumTime + susLength ?? 0; // Uses the parent strum time since it was generated from that

        // Save a reference to THIS strumline sustain clipper to be able to modify it later on
        public ClippedContainer<Sustain> Clipper;

        protected BindableBool UseLegacySpritesheet;

        protected virtual SustainSprite GetSustainBody() => new(Head, UseLegacySpritesheet);
        protected virtual SustainEnd GetSustainEnd() => new(Head, UseLegacySpritesheet);

        public Sustain(Note head)
        {
            // Recalculate the height
            Speed.BindValueChanged((v) =>
            {
                float prev = susLength;
                susLength = 0;
                Length = prev;
            });

            Head = head;
            head.BoundToSustain = true;
            // Had to chain load events since for some reason now on TestSceneSustain it throws because of missing note cache (even if cached before)
            head.OnLoadComplete += head_OnLoadComplete;

            Alpha = 0.8f;
            RelativeSizeAxes = Axes.None; // I went insane for about an hour and I only had to do this bro
            Depth = 1;
        }

        [BackgroundDependencyLoader]
        private void load(FunkinConfig config)
        {
            // so that when setting the value doesnt trigger saving
            // also we checking if its null or not to know if it was already set by an extending class
            UseLegacySpritesheet ??= new(config.Get<bool>(FunkinSetting.UseLegacyNoteSpritesheet));
        }

        private void head_OnLoadComplete(Drawable obj)
        {
            Add(Body = GetSustainBody());
            Body.OnLoadComplete += body_OnLoadComplete;
        }

        private void body_OnLoadComplete(Drawable obj)
        {
            if (UseLegacySpritesheet.Value)
                Width = Body.CurrentFrame.DisplayWidth * Body.Scale.X;
            else
                Width = Body.DrawWidth * Body.Scale.X;

            Anchor = Origin = Body.Anchor;

            // Create the end when the body is done
            Add(End = GetSustainEnd());
        }

        protected override void Update()
        {
            // TODO: Add checks to see if the stuff is alive or not (I dont think that is neccesary)
            if (Head.IsLoaded && Body.IsLoaded && End.IsLoaded)
            {
                if ((MaxHeight == 0 && Height != TargetHeight.Value))
                    MaxHeight = TargetHeight.Value;

                Height = float.Clamp(TargetHeight.Value, 0, MaxHeight);

                Body.Height = (Height - End.Height);
                if (Downscroll.Value)
                    Body.Rotation = 180;

                Y = (Head.Y + Head.AnchorPosition.Y);
                if (Downscroll.Value)
                    Y *= -1;

                if (Missed && Alpha != 0.3f)
                {
                    Alpha = 0.3f;
                    Head.Alpha = 0.3f;
                }
            }

            base.Update();
        }

        // https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/play/notes/SustainTrail.hx#L148
        public static float SustainHeight(float susLength, float scrollSpeed) => (susLength * SongConstants.PIXELS_PER_MS * scrollSpeed);

    }
}
