
using System;
using System.Collections.Generic;
using FunkinSharp.Game.Core;
using Newtonsoft.Json;

namespace FunkinSharp.Game.Funkin.Song
{
    public class SongEventData
    {
        // SongEventDataRaw https://github.com/FunkinCrew/Funkin/blob/main/source/funkin/data/song/SongData.hx#L635
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

        [JsonProperty("e")]
        public readonly string Kind;

        [JsonProperty("v", NullValueHandling = NullValueHandling.Ignore)]
        public readonly dynamic Value;

        [JsonIgnore]
        public bool Activated = false;

        public SongEventData(double time, string kind, dynamic value)
        {
            Time = time;
            Kind = kind;
            Value = value;
        }

        public double GetStepTime(bool force = false)
        {
            if (stepTime != -1 && !force) return stepTime;
            return stepTime = Conductor.Instance.GetTimeInSteps(time);
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

        // These functions crash when we try to access it through the indexer to get a value from the desired key and there is no indexer on the type (long)
        // including i dont fucking know how to cast some values properly i left them as they were (including ValueAsStruct since i aint using that one at all - for now)
        // i will most likely look for a way to rewrite these, since these are ported from haxe
        public dynamic GetDynamic(string key) => Value?[key];
        public bool? GetBool(string key) => (bool)Value?[key];
        public int? GetInt(string key)
        {
            if (Value == null) return null;
            dynamic result = (Value is not object || Value is int or long) ? Value : Value[key];
            if (result == null) return null;
            if (result is int v) return v;
            if (result is string) return int.Parse(result);
            return (int)result;
        }
        public float? GetFloat(string key)
        {
            if (Value == null) return null;
            dynamic result = (Value is not object || Value is float or long) ? Value : Value[key];
            if (result == null) return null;
            if (result is float v) return v;
            if (result is string) return float.Parse(result);
            return (float)result;
        }
        public string GetString(string key)
        {
            // we return mostly nulls here since we want to allow the event handler to set the default value in case the value is not the type its looking for
            if (Value == null) return null;
            if (Value is string s) return s;
            if (Value is not string) return null;
            return null;
        }
        public dynamic[] GetArray(string key) => (dynamic[])Value?[key];

        // buildtooltip

        public static bool operator ==(SongEventData a, SongEventData b) => (a.Time == b.Time) && (a.Kind == b.Kind) && (a.Value == b.Value);
        public static bool operator !=(SongEventData a, SongEventData b) => (a.Time != b.Time) || (a.Kind != b.Kind) || (a.Value != b.Value);

        public static bool operator >(SongEventData a, SongEventData b) => a.Time > b.Time;
        public static bool operator <(SongEventData a, SongEventData b) => a.Time < b.Time;

        public static bool operator >=(SongEventData a, SongEventData b) => a.Time >= b.Time;
        public static bool operator <=(SongEventData a, SongEventData b) => a.Time <= b.Time;

        public new string ToString() => $"SongEventData({Time}ms, {Kind}: {Value})";

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
