using System;
using System.Collections.Generic;
using System.Linq;
using FunkinSharp.Game.Funkin.Song;
using osu.Framework.Logging;
using static FunkinSharp.Game.Core.Utils.EventDelegates;

namespace FunkinSharp.Game.Core
{
    // https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/Conductor.hx

    public static partial class Conductor
    {
        /// <summary>
        ///     The current instance of the Conductor.
        ///     If one doesn't currently exist, a new one will be created.
        ///     <para/>
        ///     You can also have stuff like store a reference to the Conductor and pass it around or temporarily replace it,
        ///     or have a second Conductor running at the same time, or other weird stuff like that if you need to.
        /// </summary>
        public static InstanceConductor Instance
        {
            get
            {
                instance ??= new InstanceConductor();
                if (instance == null) { throw new NullReferenceException(); }
                return instance;
            }
            private set
            {
                if (instance != null) clearSingleton(instance);
                instance = value;
                if (instance != null) setupSingleton(instance);
            }
        }

        private static InstanceConductor instance = null;

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
            Instance = new InstanceConductor();
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
        private static void setupSingleton(InstanceConductor input)
        {
            input.OnMeasureHit += dispatchMeasureHit;
            input.OnBeatHit += dispatchBeatHit;
            input.OnStepHit += dispatchStepHit;
        }

        private static void clearSingleton(InstanceConductor input)
        {
            input.OnMeasureHit -= dispatchMeasureHit;
            input.OnBeatHit -= dispatchBeatHit;
            input.OnStepHit -= dispatchStepHit;
        }

        // The class is inside here cuz why not yea
        public class InstanceConductor
        {
            /// <summary>
            ///     Event fired when THIS Conductor instance advances to a new measure.
            /// </summary>
            public event Signal OnMeasureHit;

            /// <summary>
            ///     Event fired when THIS Conductor instance advances to a new beat.
            /// </summary>
            public event Signal OnBeatHit;

            /// <summary>
            ///     Event fired when THIS Conductor instance advances to a new step.
            /// </summary>
            public event Signal OnStepHit;

            /// <summary>
            ///     The list of time changes in the song.
            ///     <para/>
            ///     There should be at least one time change (at the beginning of the song) to define the BPM.
            /// </summary>
            private List<SongTimeChange> timeChanges;

            /// <summary>
            ///     The most recent time change for the current song position.
            /// </summary>
            public SongTimeChange CurrentTimeChange { get; private set; }

            /// <summary>
            ///     The current position in the song in milliseconds.
            ///     <para/>
            ///     Update this every frame based on the audio positioning using <see cref="InstanceConductor"/>
            /// </summary>
            public double SongPosition;

            /// <summary>
            ///     The current value set by <see cref="InstanceConductor"/> (forceBPM)
            ///     <para/>
            ///     If false, BPM is determined by time changes.
            /// </summary>
            private double bpmOverride = -1;

            /// <summary>
            ///     Beats per minute of the current song at the current time.
            /// </summary>
            public double BPM
            {
                get
                {
                    if (bpmOverride != -1) return bpmOverride;
                    if (CurrentTimeChange == null) return SongConstants.DEFAULT_BPM;
                    return CurrentTimeChange.BPM;
                }
            }

            /// <summary>
            ///     Beats per minute of the current song at the start time.
            /// </summary>
            public double StartingBPM
            {
                get
                {
                    if (bpmOverride != -1) return bpmOverride;

                    SongTimeChange timeChange = timeChanges[0];
                    if (timeChange == null) return SongConstants.DEFAULT_BPM;

                    return timeChange.BPM;
                }
            }

            /// <summary>
            ///     The numerator for the current time signature (the "3" in "3/4")
            /// </summary>
            public int TimeSignatureNumerator
            {
                get
                {
                    if (CurrentTimeChange == null) return SongConstants.DEFAULT_TIME_SIGNATURE_NUM;
                    return CurrentTimeChange.TimeSignatureNum;
                }
            }

            /// <summary>
            ///     The denominator for the current time signature (the "4" in "3/4")
            /// </summary>
            public int TimeSignatureDenominator
            {
                get
                {
                    if (CurrentTimeChange == null) return SongConstants.DEFAULT_TIME_SIGNATURE_DEN;
                    return CurrentTimeChange.TimeSignatureDen;
                }
            }

            /// <summary>
            ///     Duration of a measure in milliseconds. Calculated based on bpm.
            /// </summary>
            public double MeasureLengthMs => BeatLengthMs * TimeSignatureNumerator;

            // crochet type shi
            /// <summary>
            ///     Duration of a beat (quarter note) in milliseconds. Calculated based on bpm.
            /// </summary>
            public double BeatLengthMs => ((SongConstants.SECS_PER_MIN / BPM) * SongConstants.MS_PER_SEC);

            // stepcrochet type shi
            /// <summary>
            ///     Duration of a step (sixtennth note) in milliseconds. Calculated based on bpm.
            /// </summary>
            public double StepLengthMS => BeatLengthMs / TimeSignatureNumerator;

            /// <summary>
            ///     Current position in the song, in measures.
            /// </summary>
            public int CurrentMeasure { get; private set; }

            /// <summary>
            ///     Current position in the song, in beats.
            /// </summary>
            public int CurrentBeat { get; private set; }

            /// <summary>
            ///     Current position in the song, in steps.
            /// </summary>
            public int CurrentStep { get; private set; }

            /// <summary>
            ///     Current position in the song, in measures and fractions of a measure.
            /// </summary>
            public double CurrentMeasureTime { get; private set; }

            /// <summary>
            ///     Current position in the song, in beats and fractions of a measure.
            /// </summary>
            public double CurrentBeatTime { get; private set; }

            /// <summary>
            ///     Current position in the song, in steps and fractions of a step.
            /// </summary>
            public double CurrentStepTime { get; private set; }

            /// <summary>
            ///     An offset tied to the current chart file to compensate for a delay in the instrumental.
            /// </summary>
            public double InstrumentalOffset = 0;

            /// <summary>
            ///     The instrumental offset, in terms of steps.
            /// </summary>
            public double InstrumentalOffsetSteps => InstrumentalOffset / (((SongConstants.SECS_PER_MIN / StartingBPM) * SongConstants.MS_PER_SEC) / TimeSignatureNumerator);

            /// <summary>
            ///     An offset tied to the file format of the audio file being played.
            /// </summary>
            public double FormatOffset = 0;

            /// <summary>
            ///     An offset set by the user to compensate for input lag.
            ///     <para>TODO</para>
            /// </summary>
            public int InputOffset = 0;

            /// <summary>
            ///     An offset set by the user to compensate for audio/visual lag.
            ///     <para>TODO</para>
            /// </summary>
            public int AudioVisualOffset = 0;

            /// <summary>
            ///     The number of beats in a measure. May be fractional depending on the time signature.
            /// </summary>
            public double BeatsPerMeasure => StepsPerMeasure / SongConstants.STEPS_PER_BEAT;

            /// <summary>
            ///     The number of steps in a measure.
            /// </summary>
            public int StepsPerMeasure => (TimeSignatureNumerator / TimeSignatureDenominator * SongConstants.STEPS_PER_BEAT * SongConstants.STEPS_PER_BEAT);

            public InstanceConductor() { }

            /// <summary>
            ///     Forcibly defines the current BPM of the song.
            ///     <para/>
            ///     Useful for things like the chart editor that need to manipulate BPM in real time.
            /// </summary>
            /// <param name="bpm">The BPM to force THIS Conductor to, if -1 it will reset to the defined by <see cref="timeChanges"/>.</param>
            public void ForceBPM(double bpm = -1)
            {
                if (bpm != -1)
                    Logger.Log($"[Conductor] Forcing BPM to {bpm}", LoggingTarget.Runtime, LogLevel.Debug);
                else
                    Logger.Log($"[Conductor] Resetting BPM", LoggingTarget.Runtime, LogLevel.Debug);

                bpmOverride = bpm;
            }

            /// <summary>
            ///     Update THIS Conductor with the current song position.
            ///     <para/>
            ///     BPM, Current Step, etc. will be re-calculated based on the song position.
            /// </summary>
            /// <param name="songPos">The current position in the song in milliseconds.</param>
            /// <param name="applyOffsets">If it should apply the <see cref="InstrumentalOffset"/> + <see cref="FormatOffset"/> + <see cref="AudioVisualOffset"/></param>
            public void Update(double songPos, bool applyOffsets = true)
            {
                songPos += applyOffsets ? (InstrumentalOffset + FormatOffset + AudioVisualOffset) : 0;

                double oldMeasure = CurrentMeasure;
                double oldBeat = CurrentBeat;
                double oldStep = CurrentStep;

                SongPosition = songPos;

                CurrentTimeChange = timeChanges[0];
                if (SongPosition > 0.0)
                {
                    foreach (SongTimeChange timeChange in timeChanges)
                    {
                        if (SongPosition >= timeChange.TimeStamp) CurrentTimeChange = timeChange;
                        if (SongPosition < timeChange.TimeStamp) break;
                    }
                }

                if (CurrentTimeChange == null && bpmOverride == -1)
                {
                    Logger.Log("Conductor is broken, timeChanges is empty.", LoggingTarget.Runtime, LogLevel.Important);
                }
                else if (CurrentTimeChange != null && SongPosition > 0.0)
                {
                    CurrentStepTime = Math.Round((CurrentTimeChange.BeatTime * SongConstants.STEPS_PER_BEAT) + (SongPosition - CurrentTimeChange.TimeStamp) / StepLengthMS, 6);
                }
                else
                {
                    CurrentStepTime = Math.Round(SongPosition / StepLengthMS, 4);
                }

                CurrentBeatTime = CurrentStepTime / SongConstants.STEPS_PER_BEAT;
                CurrentMeasureTime = CurrentStepTime / StepsPerMeasure;

                CurrentStep = (int)CurrentStepTime;
                CurrentBeat = (int)CurrentBeatTime;
                CurrentMeasure = (int)CurrentMeasureTime;

                if (CurrentStep != oldStep)
                    OnStepHit?.Invoke();

                if (CurrentBeat != oldBeat)
                    OnBeatHit?.Invoke();

                if (CurrentMeasure != oldMeasure)
                    OnMeasureHit?.Invoke();
            }

            /// <summary>
            ///     Apply the <see cref="SongTimeChange"/> data from <see cref="SongMetadata"/> to THIS Conductor.
            /// </summary>
            /// <param name="songTimeChanges">The <see cref="SongMetadata.TimeChanges"/></param>
            public void MapTimeChanges(SongTimeChange[] songTimeChanges)
            {
                timeChanges = [];

                List<SongTimeChange> temp = songTimeChanges.ToList();
                temp.Sort((a, b) => a.TimeStamp.CompareTo(b.TimeStamp));

                foreach (SongTimeChange songTimeChange in temp)
                {
                    if (songTimeChange.TimeStamp < 0.0) songTimeChange.TimeStamp = 0.0;

                    if (songTimeChange.TimeStamp <= 0.0)
                        songTimeChange.BeatTime = 0.0;
                    else
                    {
                        songTimeChange.BeatTime = 0.0;
                        if (songTimeChange.TimeStamp > 0.0 && temp.Count > 0)
                        {
                            SongTimeChange prevTimeChange = temp[^1];
                            songTimeChange.BeatTime = Math.Round(prevTimeChange.BeatTime +
                                ((songTimeChange.TimeStamp - prevTimeChange.BeatTime) * prevTimeChange.BPM / SongConstants.SECS_PER_MIN / SongConstants.MS_PER_SEC),
                                4);
                        }
                    }

                    timeChanges.Add(songTimeChange);
                }

                Update(SongPosition, false);
            }

            /// <summary>
            ///     Given a time in milliseconds, returns a time in steps.
            /// </summary>
            /// <param name="ms">The time in milliseconds.</param>
            /// <returns>The time in steps.</returns>
            public double GetTimeInSteps(double ms)
            {
                if (timeChanges.Count == 0)
                    return double.Floor(ms / StepLengthMS);
                else
                {
                    double resultStep = 0.0;

                    SongTimeChange lastTimeChange = timeChanges[0];
                    foreach (SongTimeChange timeChange in timeChanges)
                    {
                        if (ms >= timeChange.TimeStamp)
                        {
                            lastTimeChange = timeChange;
                            resultStep = lastTimeChange.BeatTime * SongConstants.STEPS_PER_BEAT;
                        }
                        else
                            break;
                    }

                    double lastStepLengthMs = ((SongConstants.SECS_PER_MIN / lastTimeChange.BPM) * SongConstants.MS_PER_SEC) / TimeSignatureNumerator;
                    double resultFractionalStep = (ms - lastTimeChange.TimeStamp) / lastStepLengthMs;
                    resultStep += resultFractionalStep;

                    return resultStep;
                }
            }

            /// <summary>
            ///     Given a time in steps and fractional steps, return a time in milliseconds.
            /// </summary>
            /// <param name="stepTime">The time in steps.</param>
            /// <returns>The time in milliseconds.</returns>
            public double GetStepTimeInMs(double stepTime)
            {
                if (timeChanges.Count == 0)
                    return stepTime * StepLengthMS;
                else
                {
                    double resultMs = 0.0;

                    SongTimeChange lastTimeChange = timeChanges[0];
                    foreach (SongTimeChange timeChange in timeChanges)
                    {
                        if (stepTime >= timeChange.BeatTime * SongConstants.STEPS_PER_BEAT)
                        {
                            lastTimeChange = timeChange;
                            resultMs = lastTimeChange.TimeStamp;
                        }
                        else
                            break;
                    }

                    double lastStepLengthMs = ((SongConstants.SECS_PER_MIN / lastTimeChange.BPM) * SongConstants.MS_PER_SEC) / TimeSignatureNumerator;
                    resultMs += (stepTime - lastTimeChange.BeatTime * SongConstants.STEPS_PER_BEAT) * lastStepLengthMs;

                    return resultMs;
                }
            }

            /// <summary>
            ///     Given a time in beats and fractional beats, return a time in milliseconds.
            /// </summary>
            /// <param name="beatTime">The time in beats.</param>
            /// <returns>The time in milliseconds.</returns>
            public double GetBeatTimeInMs(double beatTime)
            {
                if (timeChanges.Count == 0)
                    return beatTime * StepLengthMS * SongConstants.STEPS_PER_BEAT;
                else
                {
                    double resultMs = 0.0;

                    SongTimeChange lastTimeChange = timeChanges[0];
                    foreach (SongTimeChange timeChange in timeChanges)
                    {
                        if (beatTime >= timeChange.BeatTime)
                        {
                            lastTimeChange = timeChange;
                            resultMs = lastTimeChange.TimeStamp;
                        }
                        else
                            break;
                    }

                    double lastStepLengthMs = ((SongConstants.SECS_PER_MIN / lastTimeChange.BPM) * SongConstants.MS_PER_SEC) / TimeSignatureNumerator;
                    resultMs += (beatTime - lastTimeChange.BeatTime) * lastStepLengthMs * SongConstants.STEPS_PER_BEAT;

                    return resultMs;
                }
            }
        }
    }
}
