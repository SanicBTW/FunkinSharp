
using System;
using FunkinSharp.Game.Funkin.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FunkinSharp.Game.Funkin.Song
{
    // In the decompiled source it uses stepTime? idk I'm gonna use strum time just in case

    public class SongEventData<T> where T : ISongEvent
    {
        [JsonProperty("t")]
        public readonly float Time;

        [JsonProperty("e")]
        public readonly string Kind;

        // Events might have different values passed into it, so in the Converter it tries to automatically set the class to access the values
        [JsonProperty("v")]
        public readonly T Value;

        // I believe this is a simple flag for the events that already happened
        [JsonIgnore]
        public bool Activated = false;

        public SongEventData(float time, string kind, T value)
        {
            Time = time;
            Kind = kind;
            Value = value;
        }
    }

    public class EventTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        // this took me so much time that now that it fully works as expected, im proud of it
        // TODO: Make it modular :trollface:
        // TODO: Make a constant Dictionary to set default values and when calling GetValue, directly get the value from the Dictionary if not found
        public override SongEventData<ISongEvent>[] ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.StartArray)
            {
                JArray jsonArray = JArray.Load(reader);
                SongEventData<ISongEvent>[] songEvents = new SongEventData<ISongEvent>[jsonArray.Count];

                for (int i = 0; i < jsonArray.Count; i++)
                {
                    JObject jObject = (JObject)jsonArray[i];

                    float eventTime = (float)jObject["t"];
                    string eventKind = (string)jObject["e"];

                    JObject rawData;

                    // made this to attempt to convert the non object value into one 
                    if (jObject["v"] is not JObject) // aint no way "is not" is valid
                    {
                        JObject cData = new JObject();

                        string propKey = "duration";
                        switch (eventKind)
                        {
                            case "FocusCamera":
                                propKey = "char";
                                break;
                        }

                        cData.Add(propKey, jObject["v"]);

                        rawData = cData;
                    }
                    else
                        rawData = (JObject)jObject["v"];

                    int duration = GetValue(rawData, "duration", 4);

                    switch (eventKind)
                    {
                        case "FocusCamera":
                            float focusX = GetValue(rawData, "x", 0);
                            float focusY = GetValue(rawData, "y", 0);
                            int focusChar = GetValue(rawData, "char", 1);
                            string focusEase = GetValue(rawData, "ease", "CLASSIC");

                            songEvents[i] = new(eventTime, eventKind,
                                new FocusCameraEvent(duration, focusX, focusY, focusChar, focusEase));
                            break;

                        case "ZoomCamera":
                            string zoomEase = GetValue(rawData, "ease", "linear"); // I guess linear is the default one?
                            float newZoom = GetValue(rawData, "zoom", 1);
                            string zoomMode = GetValue(rawData, "mode", "stage"); // stage is the default one I believe

                            songEvents[i] = new(eventTime, eventKind,
                                new ZoomCameraEvent(duration, zoomEase, newZoom, zoomMode));
                            break;

                        case "SetCameraBop":
                            float bopRate = GetValue(rawData, "rate", 1);
                            float bopIntens = GetValue(rawData, "intensity", 1);
                            songEvents[i] = new(eventTime, eventKind,
                                new SetCameraBopEvent(bopRate, bopIntens));
                            break;
                    }
                }

                return songEvents;
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        // Quick and simple wrapper to check if the key exists in the JObject and if not return the default value
        // NOW with this the code looks much much cleaner holy fuck
        public dynamic GetValue<T>(JObject from, string key, T defaultValue)
        {
            return (from.ContainsKey(key) ? from[key] : defaultValue);
        }
    }
}
