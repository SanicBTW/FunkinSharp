using System.ComponentModel;
using FunkinSharp.Game.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FunkinSharp.Game.Funkin.Song
{
    public class SongNoteData
    {
        // SongNoteDataRaw https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/data/song/SongData.hx#L864
        [JsonProperty("t")]
        private double time;

        [JsonIgnore]
        public double Time
        {
            get => time;
            set
            {
                stepTime = -1;
                time = value;
            }
        }

        [JsonIgnore]
        private double stepTime = -1;

        [JsonProperty("d")]
        public int Data;

        [JsonProperty("l", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(0)]
        private double length;

        [JsonIgnore]
        public double Length
        {
            get => length;
            set
            {
                stepLength = -1;
                length = value;
            }
        }

        [JsonIgnore]
        private double stepLength = -1;

        [JsonProperty("k", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(null)]
        private string kind;

        [JsonIgnore]
        public string Kind
        {
            get
            {
                if (kind == null || kind == "") return null;
                return kind;
            }
            set
            {
                if (value == "") value = null;
                kind = value;
            }
        }

        public SongNoteData(double time, int data, double length, string kind = "")
        {
            Time = time;
            Data = data;
            Length = length;
            Kind = kind;
        }

        public double GetStepTime(bool force = false)
        {
            if (stepTime != -1 && !force) return stepTime;
            return stepTime = Conductor.Instance.GetTimeInSteps(time);
        }

        public double GetStepLength(bool force = false)
        {
            if (Length <= 0) return 0d;
            if (stepLength != -1 && !force) return stepLength;
            return stepLength = Conductor.Instance.GetTimeInSteps(time + length) - GetStepTime();
        }

        public void SetStepLength(double value)
        {
            if (value <= 0)
                Length = 0;
            else
            {
                double endStep = GetStepTime() + value;
                double endMs = Conductor.Instance.GetStepTimeInMs(endStep);
                double lengthMs = endMs - time;

                Length = lengthMs;
            }

            stepLength = -1;
        }

        // SongNoteData abstract https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/data/song/SongData.hx#L1041

        // buildDirectionName

        [JsonIgnore]
        public bool IsHoldNote => length > 0;

        public static bool operator ==(SongNoteData a, SongNoteData b)
        {
            if (a == null) return b == null;
            if (b == null) return false;

            if (a.Kind == null || a.Kind == "")
            {
                if (b.Kind != "" && a.Kind != null) return false;
            }
            else
            {
                if (b.Kind == "" || a.Kind == null) return false;
            }

            return a.Time == b.Time && a.Data == b.Data && a.Length == b.Length;
        }

        public static bool operator !=(SongNoteData a, SongNoteData b)
        {
            if (a == null) return b == null;
            if (b == null) return false;

            if (a.Kind == "")
            {
                if (b.Kind != "") return true;
            }
            else
            {
                if (b.Kind == "") return true;
            }

            return a.Time != b.Time || a.Data != b.Data || a.Length != b.Length;
        }

        public static bool operator >(SongNoteData a, SongNoteData b)
        {
            if (b == null) return false;
            return a.Time > b.Time;
        }

        public static bool operator <(SongNoteData a, SongNoteData b)
        {
            if (b == null) return false;
            return a.Time < b.Time;
        }

        public static bool operator >=(SongNoteData a, SongNoteData b)
        {
            if (b == null) return false;
            return a.Time >= b.Time;
        }

        public static bool operator <=(SongNoteData a, SongNoteData b)
        {
            if (b == null) return false;
            return a.Time <= b.Time;
        }

        public new string ToString() => $"SongNoteData({time}ms, " +
            (Length > 0 ? $"[{Length}ms hold]" : "") +
            $" {Data} " +
            (Kind != "" ? $"[kind: {Kind}]" : "") +
            ")";

        // when i override the == && != operator, visual studio complains about not overriding these so here you go
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            SongNoteData b = (SongNoteData)obj;
            return Time != b.Time || Data != b.Data || Length != b.Length;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Time.GetHashCode();
                hash = hash * 23 + (Kind != null ? Kind.GetHashCode() : 0);
                hash = hash * 23 + Data.GetHashCode();
                return hash;
            }
        }
    }
}
