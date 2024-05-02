
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

                    JObject rawData = (JObject)jObject["v"];
                    int duration = (int)rawData["duration"];

                    switch (eventKind)
                    {
                        case "FocusCamera":
                            float focusX = (float)rawData["x"];
                            float focusY = (float)rawData["y"];
                            int focusChar = (int)rawData["char"];
                            string focusEase = (string)rawData["ease"];

                            songEvents[i] = new(eventTime, eventKind,
                                new FocusCameraEvent(duration, focusX, focusY, focusChar, focusEase));
                            break;

                        case "ZoomCamera":
                            string zoomEase = (string)rawData["ease"];
                            float newZoom = (float)rawData["zoom"];
                            string zoomMode = (string)rawData["mode"];

                            songEvents[i] = new(eventTime, eventKind,
                                new ZoomCameraEvent(duration, zoomEase, newZoom, zoomMode));
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
    }
}
