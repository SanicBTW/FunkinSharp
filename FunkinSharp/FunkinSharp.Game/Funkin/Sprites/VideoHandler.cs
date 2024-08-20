using System;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Conductors;
using FunkinSharp.Game.Funkin.Song;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Video;
using osu.Framework.Logging;

// Code coming from Silly Billy FunkinSharp Port
// https://github.com/SanicBTW/SillyBilly-FunkinSharp/tree/master/FunkinSharp/FunkinSharp.Game/Mod/Sprites/VideoHandler
namespace FunkinSharp.Game.Funkin.Sprites
{
    // Video bound to the Conductor time for pausing and staying in sync, also deleting itself from the draw tree once finished
    public partial class VideoHandler : Container
    {
        protected FunkinConductor TargetConductor;
        protected bool AudioBoundToConductor = false;

        public DrawableTrack Audio { get; protected set; }
        public Video Video { get; protected set; }

        public event Action Completed;

        public VideoHandler(FunkinConductor conductor, string videoPath, bool playAudio = true)
        {
            TargetConductor = conductor;
            setup(videoPath, playAudio);
        }

        public VideoHandler(string videoPath, bool playAudio = true)
        {
            TargetConductor = new FunkinConductor();
            AudioBoundToConductor = true;
            setup(videoPath, playAudio);
        }

        private void setup(string videoPath, bool playAudio)
        {
            if (!videoPath.EndsWith(".mp4"))
                videoPath += ".mp4";

            if (playAudio)
            {
                Track audio = Paths.GetTrack(videoPath);
                AddInternal(Audio = new DrawableTrack(audio));

                if (TargetConductor.Instrumental == null)
                    TargetConductor.Bind(audio, [], [new SongTimeChange(0, SongConstants.DEFAULT_BPM)]);
            }

            AddInternal(Video = new Video(Paths.GetStream(videoPath)));
        }

        protected override void Update()
        {
            base.Update();

            if (AudioBoundToConductor)
                TargetConductor.Update();

            if (Video != null && Video.IsAlive)
            {
                Video.PlaybackPosition = TargetConductor.SongPosition;

                if (Video.PlaybackPosition >= Video.Duration)
                {
                    RemoveInternal(Video, true);
                    if (Audio != null)
                        RemoveInternal(Audio, true);
                    Completed?.Invoke();
                }
            }

            if (Audio != null && Audio.IsAlive)
            {
                if (Video.Buffering)
                    Audio.Stop();

                if (!Video.Buffering && !Audio.IsRunning)
                    Audio.Start();

                if (!AudioBoundToConductor && // if the video audio is bound to the conductor, let that thing manage the sync
                    Audio.IsRunning &&
                    TargetConductor.ShouldResync &&
                    TargetConductor.ShouldResyncFromTime(Audio.CurrentTime))
                {
                    Audio.Stop();
                    Audio.Seek(TargetConductor.SongPosition);
                    Audio.Start();

                    Logger.Log("[VideoHandler] Resynced Video Audio");
                }
            }
        }
    }
}
