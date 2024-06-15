using System.Collections.Generic;
using System.ComponentModel;
using FunkinSharp.Game.Funkin.Song;
using Newtonsoft.Json;

namespace FunkinSharp.Game.Funkin.Compat
{
    // No structs exposed since we are converting to a 0.3 (VSlice) Chart
    // This uses the VANILLA charts, not the psych charts
    // Will probably end up using psych format? uhhh
    public static class FNFLegacy
    {
        public static BasicMetadata ConvertToBasic(string content)
        {
            SwagSong song = JsonConvert.DeserializeObject<DummyJSON>(content).Song;

            return new()
            {
                SongName = song.Song,
                Artist = "Unknown",
                BPM = song.BPM,
                Album = "volume1",
                Difficulties = [], // possible difficulties are already parsed in the registry
                GeneratedBy = SongConstants.DEFAULT_GENERATED_BY,
                ScrollSpeeds = new() { { "s", song.Speed } },
            };
        }

        // TODO: Parse events
        // TODO: Convert player3 to gf version 
        public static SongMetadata ConvertToVSliceMeta(string content)
        {
            SwagSong song = JsonConvert.DeserializeObject<DummyJSON>(content).Song;

            List<SongTimeChange> bpmChanges = [new SongTimeChange(0, song.BPM)];

            double curBPM = song.BPM;
            int totalSteps = 0;
            double totalPos = 0;
            foreach (SwagSection section in song.Notes)
            {
                if (section.ChangeBPM && section.BPM != curBPM)
                {
                    curBPM = section.BPM;
                    bpmChanges.Add(new SongTimeChange(totalPos, section.BPM));
                }

                int deltaSteps = (section.SectionBeats != -1) ? section.SectionBeats * SongConstants.STEPS_PER_BEAT : section.LengthInSteps;
                totalSteps += deltaSteps;
                totalPos += ((SongConstants.SECS_PER_MIN / curBPM) * SongConstants.MS_PER_SEC / SongConstants.STEPS_PER_BEAT) * deltaSteps;
            }

            return new(song.Song, "Unknown", "legacy")
            {
                TimeChanges = [.. bpmChanges],
                PlayData = new SongPlayData()
                {
                    Album = "volume1",
                    Stage = song.Stage,
                    Characters = new SongCharacterData(song.Player1, song.Player3 ?? song.GfVersion, song.Player2, song.NeedsVoices ? "" : "novoices")
                },
                GeneratedBy = SongConstants.DEFAULT_GENERATED_BY
            };
        }

        // TODO: Advanced parsing
        public static SongChartData ConvertToVSliceChart(string content, string diff)
        {
            SwagSong song = JsonConvert.DeserializeObject<DummyJSON>(content).Song;

            Dictionary<string, SongNoteData[]> chartNotes = [];
            List<SongNoteData> notes = [];
            List<SongEventData> events = [];

            foreach (SwagSection section in song.Notes)
            {
                foreach (var songNotes in section.SectionNotes)
                {
                    if (songNotes[1] == -1)
                        continue;

                    bool hitNote = !section.MustHitSection;
                    if (songNotes[1] > 3)
                        hitNote = section.MustHitSection;

                    // TODO: Add conversion for events, cam events should be easy afaik

                    double strumTime = songNotes[0];
                    int noteData = songNotes[1];
                    double strumLength = songNotes[2];
                    notes.Add(new(strumTime, noteData % 4, strumLength)
                    {
                        MustHit = hitNote,
                    });
                }
            }

            chartNotes[diff] = [.. notes];

            return new(new() { { diff, song.Speed } }, [.. events], chartNotes)
            {
                GeneratedBy = SongConstants.DEFAULT_GENERATED_BY
            };
        }

        private struct DummyJSON
        {
            [JsonProperty("song")]
            public SwagSong Song { get; set; }
        }

        private struct SwagSong
        {
            [JsonProperty("song")]
            public string Song { get; private set; }
            [JsonProperty("notes")]
            public SwagSection[] Notes { get; private set; }
            [JsonProperty("bpm")]
            public double BPM { get; private set; }
            [JsonProperty("needsVoices")]
            public bool NeedsVoices { get; private set; }
            [JsonProperty("speed")]
            public double Speed { get; private set; }

            [JsonProperty("player1")]
            public string Player1 { get; private set; }
            [JsonProperty("player2")]
            public string Player2 { get; private set; }
            [JsonProperty("player3")] // Psych
            public string Player3 { get; private set; }
            [JsonProperty("gfVersion")] // Psych
            public string GfVersion { get; private set; }
            [JsonProperty("stage")]
            public string Stage { get; private set; }
            [JsonProperty("validScore")]
            public bool ValidScore { get; private set; }
        }

        private struct SwagSection
        {
            [JsonProperty("sectionNotes")]
            public dynamic[] SectionNotes { get; private set; }
            [JsonProperty("lengthInSteps")]
            public int LengthInSteps { get; private set; }
            [JsonProperty("sectionBeats", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Populate)]
            [DefaultValue(-1)]
            public int SectionBeats { get; private set; }
            [JsonProperty("mustHitSection")]
            public bool MustHitSection { get; private set; }
            [JsonProperty("bpm")]
            public double BPM { get; private set; }
            [JsonProperty("changeBPM")]
            public bool ChangeBPM { get; private set; }
            [JsonProperty("altAnim")]
            public bool AltAnim { get; private set; }
        }
    }
}
