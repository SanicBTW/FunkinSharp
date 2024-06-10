using System;
using System.Collections.Generic;
using FunkinSharp.Game.Core.Windows;
using FunkinSharp.Game.Funkin.Song;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;

namespace FunkinSharp.Game.Core.Conductors
{
    // A Conductor that automatically updates the song position based on the bound song and tries to stay on sync with them
    // Basically the old Conductor this engine had but with the logic separated
    // https://github.com/SanicBTW/FunkinSharp/blob/91a205b96d03f920e34398ec4c538b7ef8ccbafc/FunkinSharp/FunkinSharp.Game/Core/Conductor.cs

    public class FunkinConductor : BaseConductor, ITrackableComponent
    {
        // Resyncing
        public bool ShouldResync = true;
        public double ResyncThreshold = 50;

        // Not an ITrack since it lacks of some fields
        public Track Instrumental { get; private set; }
        public Track[] Voices { get; private set; }

        public void Bind(Track instrumental, Track[] voices, SongTimeChange[] songTimeChanges)
        {
            Instrumental = instrumental;
            if (voices != null)
                Voices = voices;

            MapTimeChanges(songTimeChanges);
        }

        // This will track the Instrumental Time, ignoring songPos
        public override void Update(double songPos = -1, bool applyOffsets = true)
        {
            if (Instrumental != null && Instrumental.IsRunning)
            {
                if (ShouldResync)
                {
                    bool shouldResyncInst = ShouldResyncFromTime(Instrumental.CurrentTime);
                    if (shouldResyncInst || shouldResyncInst && (Voices.Length > 0 &&
                        (Voices[0] != null && ShouldResyncFromTime(Voices[0].CurrentTime)) || // Player Voices / Main Voice
                        (Voices[1] != null && ShouldResyncFromTime(Voices[1].CurrentTime)))) // Opp Voices
                    {
                        Resync();
                    }
                }

                base.Update(Instrumental.CurrentTime, applyOffsets);
            }
            else if (Instrumental != null && !Instrumental.IsRunning)
                SongPosition = Instrumental.CurrentTime; // when the instrumental stops, keep it on the same time
            else
                base.Update(0, applyOffsets);
        }

        public void Resync()
        {
            Instrumental.Stop();
            if (Voices.Length > 0)
            {
                foreach (Track voice in Voices)
                    voice.Stop();
            }

            SongPosition = Instrumental.CurrentTime;
            Instrumental.Start();

            if (Voices.Length > 0)
            {
                foreach (Track voice in Voices)
                {
                    if (SongPosition <= voice.Length)
                    {
                        voice.Seek(SongPosition);
                    }

                    voice.Start();
                }
            }

            Logger.Log("Resynced");
        }


        public bool ShouldResyncFromTime(double time) => (Math.Abs(time - SongPosition) > ResyncThreshold);

        // ITrackableComponent implementation

        bool ITrackableComponent.DAdded { get; set; }
        bool ITrackableComponent.DScheduled { get; set; }

        string ITrackableComponent.Name => "Funkin Conductor";

        FillFlowContainer ITrackableComponent.Parent { get; set; }

        private Dictionary<string, SpriteText> elements = [];

        void ITrackableComponent.Init(FillFlowContainer content)
        {
            elements["delta"] = new SpriteText()
            {
                Text = "Delta: ?"
            };

            elements["time"] = new SpriteText()
            {
                Text = "Time: ?"
            };

            elements["step"] = new SpriteText()
            {
                Text = "Step: ?"
            };

            elements["beat"] = new SpriteText()
            {
                Text = "Beat: ?"
            };

            elements["bpm"] = new SpriteText()
            {
                Text = "BPM: ?"
            };

            foreach (var el in elements)
            {
                content.Add(el.Value);
            }
        }

        bool ITrackableComponent.Refresh(double deltaTime)
        {
            foreach (var el in elements)
            {
                switch (el.Key)
                {
                    case "delta":
                        el.Value.Text = $"Delta: {deltaTime}";
                        break;
                    case "time":
                        el.Value.Text = $"Time: {(int)SongPosition}";
                        break;
                    case "step":
                        el.Value.Text = $"Step: {CurrentStep}";
                        break;
                    case "beat":
                        el.Value.Text = $"Beat: {CurrentBeat}";
                        break;
                    case "bpm":
                        el.Value.Text = $"BPM: {BPM}";
                        break;
                }
            }

            return true;
        }
    }
}
