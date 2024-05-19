using System;
using FunkinSharp.Game.Core.Conductors;
using static FunkinSharp.Game.Core.Utils.EventDelegates;

namespace FunkinSharp.Game.Core
{
    // https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/Conductor.hx
    // TODO: Be able to set a base type which the new instances will be based off, like uhh
    // baseType = typeof(FunkinConductor), new instances of the Conductor that will be used in the static access will use the base type to be created, yknow what i mean

    public static class Conductor
    {
        /// <summary>
        ///     The current instance of the Conductor.
        ///     If one doesn't currently exist, a new one will be created.
        ///     <para/>
        ///     You can also have stuff like store a reference to the Conductor and pass it around or temporarily replace it,
        ///     or have a second Conductor running at the same time, or other weird stuff like that if you need to.
        /// </summary>
        public static BaseConductor Instance
        {
            get
            {
                instance ??= new BaseConductor();
                if (instance == null) { throw new NullReferenceException(); }
                return instance;
            }
            set // not a private set since we might want to override the Instance by another type of Conductor (FunkinConductor, MusicConductor etc)
            {
                if (instance != null) clearSingleton(instance);
                instance = value;
                if (instance != null) setupSingleton(instance);
            }
        }

        private static BaseConductor instance = null;

        /// <summary>
        ///     Event fired when the current static Conductor instance advances to a new measure.
        /// </summary>
        public static event Signal OnMeasureHit;

        /// <summary>
        ///     Event fired when the current static Conductor instance advances to a new beat.
        /// </summary>
        public static event Signal OnBeatHit;

        /// <summary>
        ///     Event fired when the current static Conductor instance advances to a new step.
        /// </summary>
        public static event Signal OnStepHit;

        public static void Reset()
        {
            Instance = new BaseConductor();
        }

        // Event voids (Just an intermediate between the static events and the instance events)
        private static void dispatchMeasureHit()
        {
            OnMeasureHit?.Invoke();
        }

        private static void dispatchBeatHit()
        {
            OnBeatHit?.Invoke();
        }

        private static void dispatchStepHit()
        {
            OnStepHit?.Invoke();
        }

        // These setup events
        private static void setupSingleton(BaseConductor input)
        {
            input.OnMeasureHit += dispatchMeasureHit;
            input.OnBeatHit += dispatchBeatHit;
            input.OnStepHit += dispatchStepHit;
        }

        private static void clearSingleton(BaseConductor input)
        {
            input.OnMeasureHit -= dispatchMeasureHit;
            input.OnBeatHit -= dispatchBeatHit;
            input.OnStepHit -= dispatchStepHit;
        }
    }
}
