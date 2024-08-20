using System.Collections.Generic;
using FunkinSharp.Game.Core.Conductors;
using FunkinSharp.Game.Funkin.Song;
using Newtonsoft.Json;

namespace FunkinSharp.Game.Funkin.Compat
{
    // This type of chart is present on the "test" chart
    public static class FNFLegacy010
    {
        public static BasicMetadata ConvertToBasic(string content)
        {
            DummyJSON full = JsonConvert.DeserializeObject<DummyJSON>(content);
            SwagSong song = full.Song;

            // this will override the scanned diffs
            string[] diffs = new string[song.Notes.Count];
            song.Notes.Keys.CopyTo(diffs, 0);
            for (int i = 0; i < diffs.Length; i++)
            {
                string diff = diffs[i];
                diffs[i] = char.ToUpper(diff[0]) + diff[1..];
            }

            return new()
            {
                SongName = song.Song,
                Artist = "Unknown",
                BPM = song.BPM,
                Album = "volume1",
                Difficulties = diffs,
                GeneratedBy = full.GeneratedBy,
                ScrollSpeeds = song.Speed
            };
        }

        // TODO: Add BETTER play data conversion :fire:
        public static SongMetadata ConvertToVSliceMeta(string content)
        {
            DummyJSON full = JsonConvert.DeserializeObject<DummyJSON>(content);
            SwagSong song = full.Song;

            return new(song.Song, "Unknown", "legacy010")
            {
                TimeChanges = [new SongTimeChange(0, song.BPM)],
                PlayData = new SongPlayData()
                {
                    Album = "volume1",
                    Stage = song.Stage,
                    Characters = new SongCharacterData(song.Player1, "gf", song.Player2)
                },
                GeneratedBy = full.GeneratedBy
            };
        }

        // We only convert the target diff notes, as we arent saving this (for now)
        public static SongChartData ConvertToVSliceChart(string content, string diff)
        {
            DummyJSON full = JsonConvert.DeserializeObject<DummyJSON>(content);
            SwagSong song = full.Song;

            Dictionary<string, SongNoteData[]> chartNotes = [];
            List<SongNoteData> notes = [];
            List<SongEventData> events = [];

            // kind of dumb but its made to add a lil bit of length to the sustain based off the bpm
            double lastBPM = song.BPM;
            double totalPos = 0;
            bool? lastSectionWasHit = null;

            BaseConductor tempConductor = new BaseConductor();
            tempConductor.ForceBPM(lastBPM);
            foreach (SwagSection section in song.Notes[diff])
            {
                int deltaSteps = section.LengthInSteps;
                totalPos += ((SongConstants.SECS_PER_MIN / lastBPM) * SongConstants.MS_PER_SEC / SongConstants.STEPS_PER_BEAT) * deltaSteps;

                if (lastSectionWasHit == null || section.MustHitSection != lastSectionWasHit)
                {
                    lastSectionWasHit = section.MustHitSection;
                    Dictionary<string, dynamic> focusChar = new()
                    {
                        { "char", (section.MustHitSection ? 0 : 1) },
                        { "x", null },
                        { "y", null },
                        { "ease", null },
                        { "duration", null }
                    };

                    events.Add(new SongEventData(totalPos, "FocusCamera", focusChar));
                }

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

            return new(song.Speed, [.. events], chartNotes)
            {
                GeneratedBy = full.GeneratedBy
            };
        }

        private struct DummyJSON
        {
            [JsonProperty("song")]
            public SwagSong Song { get; set; }
            [JsonProperty("version")]
            public string Version { get; set; }
            [JsonProperty("generatedBy")]
            public string GeneratedBy { get; set; }
        }

        private struct SwagSong
        {
            [JsonProperty("player2")]
            public string Player2 { get; private set; }
            [JsonProperty("player1")]
            public string Player1 { get; private set; }
            [JsonProperty("notes")]
            public Dictionary<string, SwagSection[]> Notes { get; private set; }
            [JsonProperty("hasDialogueFile")]
            public bool HasDialogueFile { get; private set; }
            [JsonProperty("voiceList")]
            public string[] VoiceList { get; private set; }
            [JsonProperty("needsVoices")]
            public bool NeedsVoices { get; private set; }
            [JsonProperty("song")]
            public string Song { get; private set; }
            [JsonProperty("bpm")]
            public double BPM { get; private set; }
            [JsonProperty("stageDefault")]
            public string Stage { get; private set; }
            [JsonProperty("speed")]
            public Dictionary<string, double> Speed { get; private set; }
        }

        private struct SwagSection
        {
            [JsonProperty("sectionNotes")]
            public dynamic[] SectionNotes { get; private set; }
            [JsonProperty("lengthInSteps")]
            public int LengthInSteps { get; private set; }
            [JsonProperty("mustHitSection")]
            public bool MustHitSection { get; private set; }
        }
    }
}
