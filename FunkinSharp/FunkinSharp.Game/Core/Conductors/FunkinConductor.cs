using System;
using FunkinSharp.Game.Funkin.Song;
using osu.Framework.Audio.Track;
using osu.Framework.Logging;

namespace FunkinSharp.Game.Core.Conductors
{
    // A Conductor that automatically updates the song position based on the bound song and tries to stay on sync with them
    // Basically the old Conductor this engine had but with the logic separated
    // https://github.com/SanicBTW/FunkinSharp/blob/91a205b96d03f920e34398ec4c538b7ef8ccbafc/FunkinSharp/FunkinSharp.Game/Core/Conductor.cs

    public class FunkinConductor : BaseConductor
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
    }
}
