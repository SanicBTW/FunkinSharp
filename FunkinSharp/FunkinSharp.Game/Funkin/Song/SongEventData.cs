
using System;
using System.Collections.Generic;
using FunkinSharp.Game.Funkin.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FunkinSharp.Game.Funkin.Song
{
    public class SongEventData
    {
        // SongEventDataRaw https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/data/song/SongData.hx#L635
        [JsonProperty("t")]
        private float time;

        [JsonIgnore]
        public float Time
        {
            get => time;
            set
            {
                stepTime = -1;
                time = value;
            }
        }

        [JsonIgnore]
        private float stepTime = -1;

        [JsonProperty("e")]
        public readonly string Kind;

        // Events might have different values passed into it, so in the Converter it tries to automatically set the class to access the values
        [JsonProperty("v", NullValueHandling = NullValueHandling.Ignore)]
        public readonly dynamic Value;

        [JsonIgnore]
        public bool Activated = false;

        public SongEventData(float time, string kind, dynamic value)
        {
            Time = time;
            Kind = kind;
            Value = value;
        }

        // TODO
        public float GetStepTime(bool force = false)
        {
            if (stepTime != -1 && !force) return stepTime;

            return stepTime = time;
        }

        // SongEventData abstract https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/data/song/SongData.hx#L702
        public dynamic ValueAsStruct(string defaultKey = "key") // We can't really pass anonymous structures so we might need some casting when trying to access some field in a dynamic variable
        {
            if (Value == null) return new object();
            if (Value is Array)
            {
                Dictionary<string, dynamic> result = [];
                result[defaultKey] = Value;
                return result;
            }
            else if (Value is object)
            {
                return Value;
            }
            else
            {
                Dictionary<string, dynamic> result = [];
                result[defaultKey] = Value;
                return result;
            }
        }

        // handler
        // schema

        public dynamic GetDynamic(string key) => Value?[key];
        public bool? GetBool(string key) => (bool)Value?[key];
        public int? GetInt(string key)
        {
            if (Value == null) return null;
            dynamic result = (Value is not object) ? Value : Value[key];
            if (result == null) return null;
            if (result is int v) return v;
            if (result is string) return int.Parse(result);
            return (int)result;
        }
        public float? GetFloat(string key)
        {
            if (Value == null) return null;
            dynamic result = (Value is not object) ? Value : Value[key];
            if (result == null) return null;
            if (result is float v) return v;
            if (result is string) return float.Parse(result);
            return (float)result;
        }
        public string GetString(string key) => (string)Value?[key];
        public dynamic[] GetArray(string key) => (dynamic[])Value?[key];
        public bool[] GetBoolArray(string key) => (bool[])Value?[key];

        // buildtooltip

        public static bool operator ==(SongEventData a, SongEventData b) => (a.Time == b.Time) && (a.Kind == b.Kind) && (a.Value == b.Value);
        public static bool operator !=(SongEventData a, SongEventData b) => (a.Time != b.Time) || (a.Kind != b.Kind) || (a.Value != b.Value);

        public static bool operator >(SongEventData a, SongEventData b) => a.Time > b.Time;
        public static bool operator <(SongEventData a, SongEventData b) => a.Time < b.Time;

        public static bool operator >=(SongEventData a, SongEventData b) => a.Time >= b.Time;
        public static bool operator <=(SongEventData a, SongEventData b) => a.Time <= b.Time;

        public new string ToString() => $"SongEventData({Time}ms, {Kind}: {Value})";

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

            SongEventData b = (SongEventData)obj;
            return (Time == b.Time) && (Kind == b.Kind) && (Value == b.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Time.GetHashCode();
                hash = hash * 23 + (Kind != null ? Kind.GetHashCode() : 0);
                hash = hash * 23 + (Value != null ? Value.GetHashCode() : 0);
                return hash;
            }
        }
    }
}
