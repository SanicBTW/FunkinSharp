using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Containers;
using FunkinSharp.Game.Core.Utils;
using FunkinSharp.Game.Funkin.Song;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Framework.Utils;

namespace FunkinSharp.Game.Funkin.Screens
{
    // TODO: Do metadata & chart validation
    // TODO: Decouple ui & loading logic, like have an array of loaders that will get created when THIS screen is entered, to make it more dynamic and accept more chart formats
    // TODO: Make a base screen that works as intermediary (this screen) that holds a rotating sticker that could work as a loading screen for anything else
    // non fnf 0.3 formats will get converted here, at runtime and wont get saved (if the setting SaveConversions is set to true, it will use the cache to save the chart converted to 0.3)
    public partial class SongLoading : FunkinScreen
    {
        private static string regex = @"[^a-zA-Z0-9 ]";

        private string targetFormat;
        private string targetSong;
        private string targetDiff;

        private SongChartData endChart;
        private SongMetadata endMeta;
        private List<Track> tracks = [];
        private bool fired;

        private Camera camera = new(false); // World camera
        private double outTime = 1000D;

        private string[] erectDiffs = ["erect", "nightmare"];
        private string[] stickerChars = ["bf", "dad", "gf", "mom", "monster", "pico"];
        private int startinSticker = 1; // 1 - 2 - 3
        private int maxSticker = 4; // it runs to 3 on a for loop

        private bool canExit = false;
        private bool failed = false;
        private SpriteText loadText;
        private Sprite sticker;

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            loadText.FadeIn(1500D, Easing.InQuint);
            sticker.FadeIn(1500D, Easing.InQuint).OnComplete((_) =>
            {
                sticker.Loop(s => s.RotateTo(0).RotateTo(360, 3500D, Easing.OutSine));
                Scheduler.AddDelayed(() =>
                {
                    loadAssets();
                }, 1000D);
            });
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            loadText.FadeOut(1500D, Easing.OutQuint);
            sticker.FadeOut(1500D, Easing.OutQuint).OnComplete((_) =>
            {
                // sometimes it wont reach this place ??? so fuck it, jk
                // TODO: Fix transitions when failing
                canExit = true;
                Scheduler.AddDelayed(() =>
                {
                    if (failed)
                        Game.ScreenStack.Push(new ChartFormatSelect());
                    else
                        Game.ScreenStack.Push(new LiteGameplayScreen(endChart, endMeta, tracks, targetDiff, targetFormat));
                }, outTime);
            });
            return canExit;
        }

        public SongLoading(string format, string songToLoad, string diff)
        {
            targetFormat = format;
            targetSong = FormatSong(songToLoad);
            targetDiff = diff;
        }

        public static string FormatSong(string song) => Regex.Replace(song.ToLower().Replace("-", " "), regex, "").Replace(" erect", "").Replace(" ", "-");

        [BackgroundDependencyLoader]
        private void load()
        {
            // To clean tracks & sounds
            Paths.ClearStoredMemory();
            Paths.ClearUnusedMemory();

            Add(camera);

            string randChar = stickerChars[RNG.Next(0, stickerChars.Length)];
            int stickerImg = RNG.Next(startinSticker, maxSticker);

            camera.Add(loadText = new SpriteText
            {
                Font = new FontUsage(family: "RedHatDisplay", size: 40, weight: "Bold"),
                Text = $"Loading {targetSong} {targetDiff}",
                Margin = new MarginPadding(16),
                Alpha = 0,
            });

            camera.Add(new Container()
            {
                AutoSizeAxes = Axes.Both,
                Margin = new MarginPadding(32),
                Origin = Anchor.BottomRight,
                Anchor = Anchor.BottomRight,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = Colour4.Transparent,
                    },
                    sticker = new Sprite()
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Texture = Paths.GetTexture($"Textures/General/Stickers/{randChar}Sticker{stickerImg}.png", false),
                        Alpha = 0,
                        Scale = new osuTK.Vector2(0.5f)
                    }
                }
            });
        }

        private void loadAssets()
        {
            var ctor = ReflectionUtils.GetConstructorFrom<GameThread>(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, [typeof(Action), typeof(string), typeof(bool)]) ?? throw new NullReferenceException();
            GameThread loaderThread = null;

            // NOTE FOR MY DUMB SELF: the game thread runs every frame, that means that any variable inside of it will be created too, that could cause a big memory leak
            // so you will need to add some checks for the data to avoid resetting it and allocating more stuff, making the gc to run, its probably a good idea to set the speed of the thread to 1hz
            MultiCallback asyncMult = new MultiCallback(() =>
            {
                loaderThread.Exit(); // Done loading or crashed, either way close the thread

                if (fired)
                    return;

                fired = true;

                Scheduler.AddDelayed(() =>
                {
                    if (failed)
                        Game.ScreenStack.Push(new ChartFormatSelect());
                    else
                        Game.ScreenStack.Push(new LiteGameplayScreen(endChart, endMeta, tracks, targetDiff, targetFormat));
                }, outTime);
            }, $"LoadingScreen:{targetSong}");

            // i love this naming stuff fuck
            Action chartDone = asyncMult.Add("chart");
            Action metaDone = asyncMult.Add("meta");
            Action instDone = asyncMult.Add("inst");
            Action voicesDone = asyncMult.Add("mainVoices"); // holds Voices / Voices-bf, will get fired anyways on non fnf charts
            Action oppVoices = asyncMult.Add("oppVoices"); // this will get fired anyways in any format

            // the fuck is this bruh
            void failedLoad(string reason)
            {
                // TODO: Specify the stuff in the metadata for inst, voices, i think we will just ignore opp voices
                switch (reason)
                {
                    case "chart":
                        loadText.Text = $"Failed to load the chart for {targetSong} {targetDiff} on {targetFormat}";
                        break;

                    case "metadata":
                        loadText.Text = $"Failed to load the metadata of {targetSong} {targetDiff} on {targetFormat}";
                        break;

                    case "inst":
                        loadText.Text = $"Failed to load the instrumental for {targetSong} {targetDiff} on {targetFormat}";
                        break;

                    case "voices":
                        loadText.Text = $"Failed to load the Main/BF Voices for {targetSong} {targetDiff} on {targetFormat}";
                        break;
                }

                Scheduler.AddDelayed(() =>
                {
                    failed = true;
                    chartDone();
                    metaDone();
                    instDone();
                    voicesDone();
                    oppVoices();
                }, 2000D);
            }

            loaderThread = (GameThread)ctor.Invoke([() =>
            {
                if (failed)
                {
                    loaderThread.Exit();
                    return;
                }

                switch (targetFormat)
                {
                    // TODO: Better assertions
                    // TODO: Reflection???
                    // TODO: Fix error catching
                    case "FNF VSlice":
                        // all of this if's are to make sure we dont assign data every frame, i should probably set the speed of the thread to be like 1hz or sum shit but fuck it
                        if (endChart == null)
                        {
                            if ((endChart = ChartRegistry.GetChart(targetFormat, targetSong, erectDiffs.Contains(targetDiff))) != null)
                                chartDone();
                            else
                                failedLoad("chart");
                        }

                        if (endMeta == null)
                        {
                            if ((endMeta = ChartRegistry.GetMetadata(targetFormat, targetSong, erectDiffs.Contains(targetDiff))) != null)
                                metaDone();
                            else
                                failedLoad("metadata");
                        }
                        break;

                    case "FNF Legacy":
                        if (endChart == null)
                        {
                            if ((endChart = ChartRegistry.LegacyToChart(targetSong, targetDiff, targetFormat)) != null)
                                chartDone();
                            else
                                failedLoad("chart");
                        }

                        if (endMeta == null)
                        {
                            if ((endMeta = ChartRegistry.LegacyToMetadata(targetSong, targetDiff, targetFormat)) != null)
                                metaDone();
                            else
                                failedLoad("metadata");
                        }
                        break;
                }

                if (tracks.Count == 0)
                {
                    Track inst = ChartRegistry.GetInstrumental(targetFormat, targetSong, endMeta.PlayData.Characters.Instrumental);
                    tracks.Add(inst);
                    if (inst.GetType() != typeof(TrackVirtual))
                        instDone();
                    else
                        failedLoad("inst");
                }

                if (tracks.Count == 1)
                {
                    Track voices = ChartRegistry.GetVoices(targetFormat, targetSong, endMeta.PlayData.Characters.Player, endMeta.PlayData.Characters.Instrumental);
                    tracks.Add(voices);

                    // i have no other way, im sorry :pray:
                    if (voices.GetType() != typeof(TrackVirtual) || voices.GetType() == typeof(TrackVirtual) && endMeta.PlayData.Characters.Instrumental == "novoices")
                    {
                        voicesDone();

                        // quick support for psych > 0.7.2 (?) voices, if the return is a virtual then dont add it 
                        Track opps = ChartRegistry.GetVoices(targetFormat, targetSong, "Opponent", endMeta.PlayData.Characters.Instrumental);
                        if (opps.GetType() != typeof(TrackVirtual))
                            tracks.Add(opps);

                        oppVoices();
                    }
                    else
                        failedLoad("voices");
                }

                if (tracks.Count == 2 && ((targetFormat == "FNF Legacy" && targetSong == "test") || targetFormat == "FNF VSlice"))
                {
                    Track opps = ChartRegistry.GetVoices(targetFormat, targetSong, endMeta.PlayData.Characters.Opponent, endMeta.PlayData.Characters.Instrumental);
                    tracks.Add(opps);
                    oppVoices();
                }
            }, "AssetLoaderLoop", true]);
            loaderThread.Start();
        }
    }
}
