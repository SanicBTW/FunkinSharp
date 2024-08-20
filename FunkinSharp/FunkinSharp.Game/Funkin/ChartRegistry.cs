using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Utils;
using FunkinSharp.Game.Funkin.Compat;
using FunkinSharp.Game.Funkin.Song;
using Newtonsoft.Json;
using osu.Framework.Audio.Track;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;

namespace FunkinSharp.Game.Funkin
{
    // I love how this is not pushed and still has comments as if someone where to read this if I share over the source 
    // WARNING: THIS CODE NOW USES SOME OLD LEGACY CODE FOR THE ASSETS, EXPECT IT TO BREAK BUT IT WILL GET REWRITTEN SOON SOMEDAY

    // Holds basic information for the song selection
    public record BasicMetadata
    {
        public string SongName { get; set; }
        public string Artist { get; set; }
        public double BPM { get; set; }
        public string Album { get; set; }
        public string[] Difficulties { get; set; }
        public string GeneratedBy { get; set; }
        public Dictionary<string, double> ScrollSpeeds { get; set; }
    }

    // Messy chart store, only saves file references as in folders that are available, meaning theres an available chart
    // The system is just gonna be everything in the desired folder, <chart_format>/<song>/chart-metadata-audiofiles-bgfile
    // TODO: Legacy charts might have different BPM across diffs
    public static class ChartRegistry
    {
        public static string[] SUPPORTED_FORMATS => ["FNF VSlice", "FNF Legacy", "Quaver", "OSU!"];
        public static Dictionary<string, List<BasicMetadata>> CACHED_METADATA = [];
        private static Dictionary<string, int> diffOrder = new() // Osu might have it hard
        {
            { "Easy", 0 }, { "Normal", 1 }, { "Hard", 2 }, { "Erect", 3 }, { "Nightmare", 4 }
        };

        private static IStorageService cwdStorage;
        private static NativeStorage chartsFolder;

        public static void Initialize(IStorageService storage)
        {
            cwdStorage = storage;
            chartsFolder = (NativeStorage)cwdStorage.GetStorageForDirectory("charts");

            foreach (string format in SUPPORTED_FORMATS)
            {
                chartsFolder.GetStorageForDirectory(format); // only made to generate the missing folders
            }
        }

        // onFormatChange gets fired once the loop changes to the next iteration
        // onProgress gets fired everytime the dirs loop is ran
        // onDone gets fired once everything is loaded and ready
        // this function is made to get the basic metadata for available chart formats, so they can be listed in the menus
        // when the user wants to play a chart, it will get converted on runtime
        public static void Scan(Action<string> onFormatChange, Action<int, int> onProgress, Action onDone)
        {
            var ctor = ReflectionUtils.GetConstructorFrom<GameThread>(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, [typeof(Action), typeof(string), typeof(bool)]) ?? throw new NullReferenceException();
            GameThread loopThread = null;
            loopThread = (GameThread)ctor.Invoke([() =>
            {
                foreach (string format in SUPPORTED_FORMATS)
                {
                    onFormatChange(format);

                    NativeStorage folder = (NativeStorage)chartsFolder.GetStorageForDirectory(format);
                    string[] dirs = folder.GetDirectories(".").ToArray();
                    int i = 0;

                    foreach (string song in dirs)
                    {
                        switch (format)
                        {
                            case "FNF VSlice":
                                // only recurse once to get the files
                                NativeStorage first = (NativeStorage)folder.GetStorageForDirectory(song);
                                string[] metas = first.GetFiles(".", "*metadata.json").ToArray();
                                string[] erectMetas = first.GetFiles(".", "*metadata-erect.json").ToArray(); // TODO: When parsing VSlice Metadata, get the "songVariations" field and parse accordingly

                                if (song == "test")
                                {
                                    // will always be the first
                                    getList(CACHED_METADATA, "FNF Legacy", legacyToBasic(folder.GetFullPath(song)));
                                    continue;
                                }

                                if (metas.Length <= 0 && erectMetas.Length <= 0)
                                    continue;

                                BasicMetadata newMeta = vsliceToBasic(first.GetFullPath(metas[0]));
                                List<BasicMetadata> saved = getList(CACHED_METADATA, format, newMeta);
                                saved.Add(newMeta);

                                if (erectMetas.Length > 0)
                                    saved.Add(vsliceToBasic(first.GetFullPath(erectMetas[0])));
                            break;

                            case "FNF Legacy":
                                BasicMetadata newLegacyMeta = legacyToBasic(folder.GetFullPath(song));
                                if (newLegacyMeta.Difficulties == null)
                                {
                                    newLegacyMeta = null;
                                    continue;
                                }
                                List<BasicMetadata> savedLegacy = getList(CACHED_METADATA, format, newLegacyMeta);
                                savedLegacy.Add(newLegacyMeta);
                                break;

                            default:
                                break;
                        }

                        i++;
                        onProgress(i, dirs.Length);
                    }
                }

                onDone();
                loopThread.Exit();
            }, "ChartRegistryLoop", true]);
            loopThread.Start();
        }

        private static List<T> getList<T>(Dictionary<string, List<T>> from, string key, T startingValue)
        {
            List<T> ret = [];

            if (from.TryGetValue(key, out List<T> saved))
                ret = saved;
            else
                from.Add(key, [startingValue]);

            return ret;
        }

        private static BasicMetadata vsliceToBasic(string fileDir)
        {
            BasicMetadata ret;

            using (StreamReader reader = new StreamReader(chartsFolder.GetStream(fileDir)))
            {
                SongMetadata vsliceMeta = JsonConvert.DeserializeObject<SongMetadata>(reader.ReadToEnd());
                string[] Difficulties = [.. vsliceMeta.PlayData.Difficulties];

                for (int i = 0; i < Difficulties.Length; i++)
                {
                    string diff = Difficulties[i];
                    Difficulties[i] = char.ToUpper(diff[0]) + diff[1..];
                }

                ret = new()
                {
                    SongName = vsliceMeta.SongName,
                    Artist = vsliceMeta.Artist,
                    BPM = vsliceMeta.TimeChanges[0].BPM,
                    Album = vsliceMeta.PlayData.Album,
                    Difficulties = Difficulties,
                    GeneratedBy = vsliceMeta.GeneratedBy
                };
            }

            // this is fucking up my life actually, why couldnt the scroll speeds be inside the metadata file :sob:
            fileDir = fileDir.Replace("metadata", "chart");
            if (chartsFolder.Exists(fileDir))
            {
                using (StreamReader reader = new StreamReader(chartsFolder.GetStream(fileDir)))
                {
                    SongChartData vsliceChart = JsonConvert.DeserializeObject<SongChartData>(reader.ReadToEnd());
                    ret.ScrollSpeeds = vsliceChart.ScrollSpeeds;
                }
            }

            return ret;
        }

        private static BasicMetadata legacyToBasic(string folderPath)
        {
            BasicMetadata ret = new()
            {
                ScrollSpeeds = [] // so it aint null later
            };

            string[] files = chartsFolder.GetFiles(folderPath, "*.json").ToArray();
            List<string> diffs = [];

            foreach (string file in files)
            {
                string targetFile = file.Split(Path.DirectorySeparatorChar)[^1];

                // i dont fucking know how this works tbh lol
                string fileDiff = "Normal";
                string diff = "Normal";
                if (targetFile.Contains('-'))
                {
                    fileDiff = targetFile.Split("-")[^1].Replace(".json", "").Trim();
                    fileDiff = char.ToUpper(fileDiff[0]) + fileDiff[1..];
                }

                if (diffOrder.ContainsKey(fileDiff))
                    diff = fileDiff;

                if (!diffs.Contains(diff) && diffOrder.TryGetValue(diff, out int place))
                {
                    if (diffs.Count >= place)
                        diffs.Insert(place, diff);
                    else
                        diffs.Add(diff);
                }
                ret.Difficulties = [.. diffs];

                using (StreamReader reader = new StreamReader(chartsFolder.GetStream(file)))
                {
                    BasicMetadata chartBasicMeta = new();

                    if (targetFile.Contains("test"))
                    {
                        chartBasicMeta = FNFLegacy010.ConvertToBasic(reader.ReadToEnd());
                        // override diffs
                        ret.Difficulties = chartBasicMeta.Difficulties;
                        ret.ScrollSpeeds = chartBasicMeta.ScrollSpeeds;
                    }
                    else
                    {
                        chartBasicMeta = FNFLegacy.ConvertToBasic(reader.ReadToEnd());
                        ret.ScrollSpeeds[diff.ToLower()] = chartBasicMeta.ScrollSpeeds["s"];
                    }

                    BasicMetadata copy = ret;
                    ret = new()
                    {
                        SongName = chartBasicMeta.SongName,
                        Artist = chartBasicMeta.Artist,
                        BPM = chartBasicMeta.BPM,
                        Album = chartBasicMeta.Album,
                        Difficulties = copy.Difficulties,
                        GeneratedBy = chartBasicMeta.GeneratedBy,
                        ScrollSpeeds = copy.ScrollSpeeds,
                    };
                    copy = null;
                }
            }

            return ret;
        }

        // TODO: Decouple the checks, for example automatically set the diff n shit, instead of copy pasting bruh
        // TODO: Get any sound file, even if its mp3 or ogg, proper check that or sum lmao

        public static SongChartData LegacyToChart(string song, string diff, string format = "FNF Legacy")
        {
            if (song == "test")
                format = "FNF VSlice";

            NativeStorage charts = (NativeStorage)(chartsFolder.GetStorageForDirectory(format).GetStorageForDirectory(song));

            string prevDiff = diff;
            if (diff.ToLower() != "normal" || diff.ToLower() == "normal" && charts.Exists(Path.Join(".", $"{song}-{diff}.json"))) // we checking if -normal exists when the diff is normal, so if it exists we dont casually ignore it
                diff = $"-{diff}";
            else
                diff = ""; // normal diffs are non suffixed files - most of the times

            string path = Path.Join(".", $"{song}{diff}.json");
            if (!charts.Exists(path))
                return null;

            using (StreamReader reader = new StreamReader(charts.GetStream(path)))
            {
                if (format == "FNF VSlice")
                    return FNFLegacy010.ConvertToVSliceChart(reader.ReadToEnd(), prevDiff);
                else
                    return FNFLegacy.ConvertToVSliceChart(reader.ReadToEnd(), prevDiff);
            }
        }

        public static SongMetadata LegacyToMetadata(string song, string diff, string format = "FNF Legacy")
        {
            if (song == "test")
                format = "FNF VSlice";

            NativeStorage charts = (NativeStorage)(chartsFolder.GetStorageForDirectory(format).GetStorageForDirectory(song));

            if (diff.ToLower() != "normal" || diff.ToLower() == "normal" && charts.Exists(Path.Join(".", $"{song}-{diff}.json"))) // we checking if -normal exists when the diff is normal, so if it exists we dont casually ignore it
                diff = $"-{diff}";
            else
                diff = ""; // normal diffs are non suffixed files - most of the times

            string path = Path.Join(".", $"{song}{diff}.json");
            if (!charts.Exists(path))
                return null;

            using (StreamReader reader = new StreamReader(charts.GetStream(path)))
            {
                if (format == "FNF VSlice")
                    return FNFLegacy010.ConvertToVSliceMeta(reader.ReadToEnd());
                else
                    return FNFLegacy.ConvertToVSliceMeta(reader.ReadToEnd());
            }
        }

        // Using legacy code, might break
        // THESE FUNCTIONS ARE ONLY MEANT TO BE RUN ON VSLICE CHARTS
        public static SongChartData GetChart(string format, string song, bool erectChart)
        {
            string suffix = erectChart ? "-erect" : "";
            NativeStorage charts = (NativeStorage)(chartsFolder.GetStorageForDirectory(format).GetStorageForDirectory(song));
            string path = Path.Join(".", $"{song}-chart{suffix}.json");
            if (!charts.Exists(path))
                return null;

            using (StreamReader reader = new StreamReader(charts.GetStream(path)))
            {
                return JsonConvert.DeserializeObject<SongChartData>(reader.ReadToEnd());
            }
        }

        public static SongMetadata GetMetadata(string format, string song, bool erectMeta)
        {
            string suffix = erectMeta ? "-erect" : "";
            NativeStorage charts = (NativeStorage)(chartsFolder.GetStorageForDirectory(format).GetStorageForDirectory(song));
            string path = Path.Join(".", $"{song}-metadata{suffix}.json");
            if (!charts.Exists(path))
                return null;

            using (StreamReader reader = new StreamReader(charts.GetStream(path)))
            {
                return JsonConvert.DeserializeObject<SongMetadata>(reader.ReadToEnd());
            }
        }

        // TODO: Accept preview time from charts on basic meta shi
        public static Track GetInstPreview(string format, string song, string instrumental = null)
        {
            string suffix = instrumental != null && instrumental.Length > 1 ? $"-{instrumental}" : "";
            if (song == "test")
                format = "FNF VSlice";
            if (format != "FNF VSlice")
                suffix = "";

            NativeStorage songs = (NativeStorage)(chartsFolder.GetStorageForDirectory(format).GetStorageForDirectory(song));
            string name = $"Inst-{song}";
            string path = Path.Join(".", $"Inst{suffix}.ogg"); // Most likely to be an OGG file
            if (!songs.Exists(path))
            {
                Logger.Log($"Couldn't find the Instrumental Variation (Looking for {suffix})", LoggingTarget.Runtime, LogLevel.Error);
                return new TrackVirtual(double.PositiveInfinity, $"Virtual:{name}");
            }

            Track preview = AssetFactory.CreateTrack(songs.GetStream(path), name);
            preview.Volume.Value = 0;
            // reset points are handled in the song selection screen itself
            return Paths.AddTrack(preview);
        }
        
        public static Track GetInstrumental(string format, string song, string instrumental = null)
        {
            string suffix = instrumental != null && instrumental.Length > 1 ? $"-{instrumental}" : "";
            if (song == "test")
                format = "FNF VSlice";
            if (format != "FNF VSlice")
                suffix = "";

            NativeStorage songs = (NativeStorage)(chartsFolder.GetStorageForDirectory(format).GetStorageForDirectory(song));
            string name = $"Inst-{song}";
            string path = Path.Join(".", $"Inst{suffix}.ogg"); // Most likely to be an OGG file
            if (!songs.Exists(path))
            {
                Logger.Log($"Couldn't find the Instrumental Variation (Looking for {suffix})", LoggingTarget.Runtime, LogLevel.Error);
                return new TrackVirtual(double.PositiveInfinity, $"Virtual:{name}");
            }

            return Paths.AddTrack(AssetFactory.CreateTrack(songs.GetStream(path), name));
        }

        public static Track GetVoices(string format, string song, string character, string variant = null)
        {
            string suffix = variant != null && variant.Length > 1 ? $"-{variant}" : "";
            if (song == "test")
                format = "FNF VSlice";

            NativeStorage songs = (NativeStorage)(chartsFolder.GetStorageForDirectory(format).GetStorageForDirectory(song));
            if (format == "FNF Legacy")
            {
                string legpath = Path.Join(".", "Voices.ogg"); // Most likely to be an OGG file
                if (character == "Opponent")
                    legpath = legpath.Replace("Voices", "Voices-Opponent");
                if (!songs.Exists(legpath))
                {
                    Logger.Log($"Couldn't find the Voices for {song}");
                    return new TrackVirtual(double.PositiveInfinity, $"Virtual:{song}");
                }

                return Paths.AddTrack(AssetFactory.CreateTrack(songs.GetStream(legpath), Path.GetFileName(legpath)));
            }

            string name = $"Voices-{song}{character}";
            string path = Path.Join(".", $"Voices-{character}{suffix}.ogg"); // Most likely to be an OGG file
            if (!songs.Exists(path) && character.Contains('-'))
            {
                if (character.Contains('-'))
                {
                    string[] parts = character.Split("-");
                    character = string.Join('-', parts.Take(parts.Length - 1));
                    return GetVoices(format, song, character, variant);
                }
                else
                {
                    Logger.Log($"Couldn't find the Voices for {character} of variant {suffix} (Looking for {character}{suffix}");
                    return new TrackVirtual(double.PositiveInfinity, $"Virtual:{name}");
                }
            }

            return Paths.AddTrack(AssetFactory.CreateTrack(songs.GetStream(path), name));
        }
    }
}
