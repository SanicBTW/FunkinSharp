using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using FunkinSharp.Game.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osuTK.Input;

namespace FunkinSharp.Game.Funkin
{
    // This code is heavily based off my own input from my Haxe fnf engine https://github.com/SanicBTW/Just-Another-FNF-Engine/blob/master/source/backend/input/Controls.hx
    public class FunkinKeybinds : ConfigManager<FunkinAction>
    {
        public const string FILENAME = "FunkinKeybinds.json";
        protected readonly IDictionary<FunkinAction, object> DefaultOverrides;
        private readonly Storage storage;

        public ReadOnlyDictionary<FunkinAction, Key[]> DefaultKeys = new(new Dictionary<FunkinAction, Key[]>()
        {
            { FunkinAction.CONFIRM, [Key.Enter, Key.KeypadEnter] },
            { FunkinAction.BACK, [Key.Escape, Key.Escape] },
            { FunkinAction.RESET, [Key.R, Key.R] },
            { FunkinAction.PAUSE, [Key.Enter, Key.KeypadEnter] },

            { FunkinAction.UI_LEFT, [Key.A, Key.Left] },
            { FunkinAction.UI_DOWN, [Key.S, Key.Down] },
            { FunkinAction.UI_UP, [Key.W, Key.Up] },
            { FunkinAction.UI_RIGHT, [Key.D, Key.Right] },

            { FunkinAction.NOTE_LEFT, [Key.A, Key.Left] },
            { FunkinAction.NOTE_DOWN, [Key.S, Key.Down] },
            { FunkinAction.NOTE_UP, [Key.W, Key.Up] },
            { FunkinAction.NOTE_RIGHT, [Key.D, Key.Right] },

            { FunkinAction.VOLUME_UP, [Key.KeypadPlus, Key.BracketRight] },
            { FunkinAction.VOLUME_DOWN, [Key.KeypadMinus, Key.Slash] },
            { FunkinAction.VOLUME_MUTE, [Key.Keypad0, Key.Number0] }

        });
        public Dictionary<FunkinAction, Key[]> Actions = new();

        protected override void InitialiseDefaults()
        {
            foreach (var defaultBinds in DefaultKeys)
            {
                SetDefault(defaultBinds.Key, defaultBinds.Value[0], defaultBinds.Value[1]);
            };
        }

        public FunkinKeybinds(Storage storage, IDictionary<FunkinAction, object> defaultOverrides = null)
            : base(defaultOverrides)
        {
            this.storage = storage;
            DefaultOverrides = defaultOverrides;

            InitialiseDefaults();
            Load();

            Save();
        }

        protected Bindable<Key[]> SetDefault(FunkinAction lookup, Key defaultKey, Key altKey)
        {
            Key[] value = GetDefault<Key[]>(lookup, [defaultKey, altKey]);

            Bindable<Key[]> bindable = GetOriginalBindable<Key[]>(lookup);

            if (bindable == null)
                bindable = Set(lookup, value);
            else
                bindable.Value = value;

            bindable.Default = value;
            Actions.Add(lookup, value);

            return bindable;
        }

        protected TValue GetDefault<TValue>(FunkinAction lookup, TValue fallback)
        {
            if (DefaultOverrides != null && DefaultOverrides.TryGetValue(lookup, out object found))
                return (TValue)found;

            return fallback;
        }

        protected Bindable<TValue> Set<TValue>(FunkinAction lookup, TValue value)
        {
            Bindable<TValue> bindable = new Bindable<TValue>(value);
            AddBindable(lookup, bindable);
            return bindable;
        }

        protected override void PerformLoad()
        {
            if (storage.Exists(FILENAME))
            {
                try
                {
                    using (Stream stream = storage.GetStream(FILENAME, FileAccess.Read, FileMode.Open))
                    using (var sr = new StreamReader(stream))
                    {
                        ActionObject[] keys = JsonConvert.DeserializeObject<DummyJSON>(sr.ReadToEnd()).Keys;
                        foreach (ActionObject action in keys)
                        {
                            Actions[action.Action] = [action.Key, action.AltKey];
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Error occurred when parsing keybinds");
                }
            }
        }

        protected override bool PerformSave()
        {
            try
            {
                using (var stream = storage.CreateFileSafely(FILENAME))
                using (var sw = new StreamWriter(stream))
                {
                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(new StringEnumConverter()); // To save the value as the enum name rather than the enum index
                    settings.Formatting = Formatting.Indented;

                    List<ActionObject> temp = [];
                    foreach (var entry in Actions)
                    {
                        temp.Add(new ActionObject(entry.Key, entry.Value[0], entry.Value[1]));
                    }

                    ActionObject[] keys = [.. temp];
                    DummyJSON json = new DummyJSON(keys);
                    sw.Write(JsonConvert.SerializeObject(json, settings));
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error occurred when saving keybinds");
            }

            return false;
        }

        public bool ShowOnExplorer() => storage.PresentFileExternally(FILENAME);

        private struct DummyJSON
        {
            [JsonProperty("keybinds")]
            public readonly ActionObject[] Keys;

            public DummyJSON(ActionObject[] keys)
            {
                Keys = keys;
            }
        }

        private struct ActionObject
        {
            [JsonProperty("action")]
            public readonly FunkinAction Action;

            [JsonProperty("key")]
            public readonly Key Key;

            [JsonProperty("alt")]
            public readonly Key AltKey;

            public ActionObject(FunkinAction action, Key key, Key altKey)
            {
                Action = action;
                Key = key;
                AltKey = altKey;
            }
        }
    }

    // TODO?: Better naming?
    public enum Actors
    {
        // Volume keybinds will block any dispatch to the actors
        UI, // All UI_ prefixed actions including "Confirm", "Back" and "Reset"
        NOTE, // All NOTE_ prefixed actions including "Reset" and "Pause"
        NONE, // Any action that is pressed WON'T be dispatched
    }

    public enum FunkinAction
    {
        [StringValue("confirm")]
        CONFIRM,
        [StringValue("back")]
        BACK,
        [StringValue("reset")]
        RESET,
        [StringValue("pause")]
        PAUSE,

        [StringValue("ui_left")]
        UI_LEFT,
        [StringValue("ui_down")]
        UI_DOWN,
        [StringValue("ui_up")]
        UI_UP,
        [StringValue("ui_right")]
        UI_RIGHT,

        [StringValue("note_left")]
        NOTE_LEFT,
        [StringValue("note_down")]
        NOTE_DOWN,
        [StringValue("note_up")]
        NOTE_UP,
        [StringValue("note_right")]
        NOTE_RIGHT,

        [StringValue("volume_up")]
        VOLUME_UP,
        [StringValue("volume_down")]
        VOLUME_DOWN,
        [StringValue("volume_mute")]
        VOLUME_MUTE,
    }
}
