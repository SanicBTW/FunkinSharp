using FunkinSharp.Game.Core;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;

namespace FunkinSharp.Game.Funkin
{
    // This g is full of questionable choices but so far works properly
    // TODO: Add required methods for manipulating the Container (Add, Remove, etc)
    // TODO: Add transitions
    public partial class FunkinScreen : Screen
    {
        private Container content;
        protected virtual Container Content => GenerateContainer();

        public virtual bool CursorVisible
        {
            get
            {
                FunkinSharpGame game = (FunkinSharpGame)Game;
                return game.Cursor.Cursor.State.Value == Visibility.Visible;
            }

            set
            {
                FunkinSharpGame game = (FunkinSharpGame)Game;
                game.Cursor.Cursor.State.Value = (value) ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public int CurStep => Conductor.Instance.CurrentStep;

        public int CurBeat => Conductor.Instance.CurrentBeat;

        public virtual Container GenerateContainer()
        {
            // There's already an existing container for the screen!
            if (content != null)
                return content;

            content = new()
            {
                Name = "FunkinScreenContent",
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
            };
            InternalChild = content;
            return content;
        }

        // If overriden, call base so the Conductor Event listeners are properly added
        public override void OnEntering(ScreenTransitionEvent e)
        {
            Conductor.OnStepHit += StepHit;
            Conductor.OnBeatHit += BeatHit;
            Conductor.OnMeasureHit += MeasureHit;

            base.OnEntering(e);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            Conductor.OnStepHit -= StepHit;
            Conductor.OnBeatHit -= BeatHit;
            Conductor.OnMeasureHit -= MeasureHit;

            return base.OnExiting(e);
        }

        // Override this to change the location of the drawable that is going to be added to the screen
        public virtual void Add(Drawable drawable)
        {
            Content.Add(drawable);
        }

        // No need to call base on these methods since they do literally nothing
        public virtual void StepHit() { }

        public virtual void BeatHit() { }

        public virtual void MeasureHit() { }
    }
}
