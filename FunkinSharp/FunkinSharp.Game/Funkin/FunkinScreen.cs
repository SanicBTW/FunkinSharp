using System.Collections.Generic;
using System.Linq;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Utils;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using static FunkinSharp.Game.Core.Utils.EventDelegates;

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

        private protected int CurStep => Conductor.Instance.CurrentStep;

        private protected int CurBeat => Conductor.Instance.CurrentBeat;

        [Resolved]
        protected new FunkinSharpGameBase Game { get; private set; } = null!;

        public Actors TargetActions = Actors.UI;
        public event ActionUpdate OnActionPressed;
        public event ActionUpdate OnActionReleased;

        public Dictionary<FunkinAction, bool> HoldingActions = new()
        {
            { FunkinAction.CONFIRM, false },
            { FunkinAction.BACK, false },
            { FunkinAction.RESET, false },
            { FunkinAction.PAUSE, false },

            { FunkinAction.UI_LEFT, false },
            { FunkinAction.UI_UP, false },
            { FunkinAction.UI_DOWN, false },
            { FunkinAction.UI_RIGHT, false },

            { FunkinAction.NOTE_LEFT, false },
            { FunkinAction.NOTE_UP, false },
            { FunkinAction.NOTE_DOWN, false },
            { FunkinAction.NOTE_RIGHT, false },
        };

        protected override bool Handle(UIEvent e)
        {
            switch (e)
            {
                case KeyDownEvent key:
                    if (key.Repeat)
                        return true;

                    foreach (var action in Game.FunkinKeybinds.Actions)
                    {
                        if (action.Value.Contains(key.Key))
                        {
                            HoldingActions[action.Key] = true;


                            switch (TargetActions)
                            {
                                case Actors.UI:
                                    if (EnumExtensions.GetString(action.Key).StartsWith("ui_") || action.Key == FunkinAction.CONFIRM || action.Key == FunkinAction.BACK || action.Key == FunkinAction.RESET)
                                        OnActionPressed?.Invoke(action.Key);
                                    break;

                                case Actors.NOTE:
                                    if (EnumExtensions.GetString(action.Key).StartsWith("note_") || action.Key == FunkinAction.PAUSE || action.Key == FunkinAction.RESET)
                                        OnActionPressed?.Invoke(action.Key);
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                    return true;

                case KeyUpEvent key:
                    foreach (var action in Game.FunkinKeybinds.Actions)
                    {
                        if (action.Value.Contains(key.Key))
                        {
                            HoldingActions[action.Key] = false;
                            switch (TargetActions)
                            {
                                case Actors.UI:
                                    if (EnumExtensions.GetString(action.Key).StartsWith("ui_") || action.Key == FunkinAction.CONFIRM || action.Key == FunkinAction.BACK || action.Key == FunkinAction.RESET)
                                        OnActionReleased?.Invoke(action.Key);
                                    break;

                                case Actors.NOTE:
                                    if (EnumExtensions.GetString(action.Key).StartsWith("note_") || action.Key == FunkinAction.PAUSE || action.Key == FunkinAction.RESET)
                                        OnActionReleased?.Invoke(action.Key);
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                    return true;

                default:
                    return base.Handle(e);
            }
        }

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

        public override void OnResuming(ScreenTransitionEvent e)
        {
            this.FadeIn(500D, Easing.InQuint);
            base.OnResuming(e);
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            this.FadeOut(500D, Easing.OutQuint);
            base.OnSuspending(e);
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
