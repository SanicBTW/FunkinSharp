using System;
using System.Collections.Generic;
using FunkinSharp.Game.Core.Utils;
using osu.Framework.Audio.Track;
using osu.Framework.Logging;
using static FunkinSharp.Game.Core.Utils.EventDelegates;

namespace FunkinSharp.Game.Core
{
    // https://github.com/SanicBTW/Just-Another-FNF-Engine/blob/master/source/backend/Conductor.hx
    // Also using some of my old prototype code

    public readonly struct BPMChangeEvent
    {
        public readonly int StepTime;
        public readonly double SongTime;
        public readonly double BPM;
        public readonly double StepCrochet = double.NaN;

        public BPMChangeEvent(int stepTime, double songTime, double bpm)
        {
            StepTime = stepTime;
            SongTime = songTime;
            BPM = bpm;
        }

        public BPMChangeEvent(int stepTime, double songTime, double bpm, double stepCrochet)
        {
            StepTime = stepTime;
            SongTime = songTime;
            BPM = bpm;
            StepCrochet = stepCrochet;
        }
    }

    // Everything is a property wtf :sob:
    public static class Conductor 
    {
        // Timings
        public static double Time { get; private set; } = 0;

        public static int Step { get; private set; } = 0;
        public static double DecStep { get; private set; } = 0;

        public static int Beat { get; private set; } = 0;
        public static double DecBeat { get; private set; } = 0;

        // BPM

        private static double bpm = 0;
        public static double BPM
        {
            get => bpm;
            set
            {
                Crochet = ConductorUtils.CalculateCrochet(value);
                StepCrochet = Crochet / 4;
                bpm = value;
            }
        }

        public static double Crochet { get; private set; } = 0;
        public static double StepCrochet { get; private set; } = 0;
        public static List<BPMChangeEvent> BPMChanges { get; private set; } = [];

        // Resync
        public static bool ShouldResync = true;
        public static double ResyncThreshold = 50; // Should I make it a bindable?? nah i dont think i have to

        private static int lastStepHit = -1;
        private static int lastBeatHit = -1;

        // Events
        public static event IntValueUpdate OnStepHit;
        public static event IntValueUpdate OnBeatHit;

        public static event BPMValueUpdate OnBPMChange;

        // I should totally decouple the fnf logic off the conductor
        // FNF - Speed
        public const float RATE = 0.45f; // Idk if this could break the whole game logic so im putting it as a constant

        private static float speed;
        public static float Speed
        {
            get => speed;
            set => speed = float.Round(RATE * value, 2);
        }

        // FNF - Tracking object and Song Format
        // sanco here, to beggin with I thought of binding a Clock to the Conductor and on update just do the thing BUT
        // now I thought of binding a Track (Sound) and call Update on FunkinScreen (When the flag ConductorActive flag is true)
        // passing the MS and being able to track also the time of the Track so we can be on sync (most likely)
        public static ITrack Instrumental { get; private set; }
        public static List<ITrack> Voices { get; private set; } = [];

        // V0.3 Funkin Chart Format Here

        // FNF - Input, these are gonna go elsewhere so i aint putting them here

        // da real shi
        public static void Bind(ITrack instrumental, List<ITrack> voices = null)
        {
            Instrumental = instrumental;
            if (voices != null)
                Voices = voices;

            // BPM setting and chart binding
        }

        public static void ChangeBPM(double newBPM, bool dontResetBeat = true)
        {
            if (Crochet != 0 && dontResetBeat)
            {
                BPMChanges.Add(new BPMChangeEvent(
                        stepTime: Step,
                        songTime: Time,
                        bpm: newBPM,
                        stepCrochet: StepCrochet
                ));

                // I doubt this will work
                BPMChanges.Sort(ConductorUtils.CompareBPMChanges);
            }

            OnBPMChange?.Invoke(BPM, newBPM);
            BPM = newBPM;
        }

        private static double offset = 0; // i put it here just in case i forget about offsets lol

        // Delta time passes time since last frame in MS so the conversion from HaxeFlixel is not needed (elapsed * 1000)
        public static void Update(double deltaTime)
        {
            if (Instrumental != null && Instrumental.IsRunning)
            {
                // Steps
                BPMChangeEvent lastChange = ConductorUtils.GetBPMChange(Time);
                double stepConver = ((Time - offset) - lastChange.SongTime) / lastChange.StepCrochet;
                DecStep = lastChange.StepTime + stepConver;
                Step = lastChange.StepTime + (int)stepConver;

                // Beats
                DecBeat = DecStep / 4;
                Beat = Step / 4;

                // Resync and event dispatching
                if (Step > lastStepHit)
                {
                    // Resync
                    if (ShouldResyncFromTime(Instrumental.CurrentTime) || ShouldResyncFromTime(Instrumental.CurrentTime) && (Voices.Count > 0 &&
                        (Voices[0] != null && ShouldResyncFromTime(Voices[0].CurrentTime)) || // BF Voices / Main Voice
                        (Voices[1] != null && ShouldResyncFromTime(Voices[1].CurrentTime)))) // Dad Voices
                    {
                        Resync();
                    }

                    OnStepHit?.Invoke(Step);
                    lastStepHit = Step;
                }

                if (Beat > lastBeatHit)
                {
                    if (Step % 4 == 0)
                        OnBeatHit?.Invoke(Beat);
                    lastBeatHit = Beat;
                }

                Time += deltaTime;
            }
        }

        public static void Reset()
        {
            BPMChanges = [];
            Time = Step = Beat = 0;
            lastStepHit = lastBeatHit = -1;
        }

        public static void Resync()
        {
            if (Voices.Count > 0)
            {
                foreach (ITrack voice in Voices)
                    voice.Stop();
            }

            Instrumental.Start();
            Time = Instrumental.CurrentTime;

            if (Voices.Count > 0)
            {
                foreach (ITrack voice in Voices)
                {
                    if (Time <= voice.Length)
                    {
                        voice.Seek(Time);
                    }

                    voice.Start();
                }
            }

            Logger.Log("Resynced");
        }

        // Basic enough
        public static bool ShouldResyncFromTime(double time) => (Math.Abs(time - Time) > ResyncThreshold);
    }
}
