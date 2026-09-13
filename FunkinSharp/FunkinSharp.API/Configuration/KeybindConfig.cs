using FunkinSharp.API.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Input.Bindings;
using osu.Framework.Logging;
using osu.Framework.Platform;

namespace FunkinSharp.API.Configuration;

// https://github.com/SanicBTW/LivinOnSweets/blob/qrewrite/LivinOnSweets/LivinOnSweets.API/Configuration/KeybindsConfig.cs
public class KeybindConfig : ConfigManager<FunkinAction>
{
    public const string FILE_NAME = "keybinds.json";

    private readonly Storage cStorage;

    private readonly IDictionary<FunkinAction, object?> defaultKeys;

    private readonly BindableList<IKeyBinding> loadedKeybinds = [];
    public IBindableList<IKeyBinding> LoadedKeybinds => loadedKeybinds;

    private bool loadError;

    public KeybindConfig(Storage storage, IDictionary<FunkinAction, object?>? defaultOverrides = null)
        : base(defaultOverrides)
    {
        cStorage = storage;
        defaultKeys = defaultOverrides ?? new Dictionary<FunkinAction, object?>();
        Load();

        if (!loadError) return;

        bool success = Save();
        if (!success)
            throw new ApplicationException("failed to save the default keybinds");

        loadError = false;

        Load();
        if (loadError)
            throw new ApplicationException("failed to load keybinds from the new configuration file");
    }

    protected override void PerformLoad()
    {
        if (!cStorage.Exists(FILE_NAME))
        {
            loadError = true;
            return;
        }

        try
        {
            using Stream stream = cStorage.GetStream(FILE_NAME, FileAccess.Read, FileMode.Open);
            using StreamReader reader = new StreamReader(stream);

            // actions, keys, keybinds, binds, whatever man
            ActionEntry[] actions = JsonConvert.DeserializeObject<JsonStruct>(reader.ReadToEnd()).Actions;
            List<IKeyBinding> keybinds = [];

            foreach (ActionEntry action in actions)
            {
                if (action.IsCombination)
                    keybinds.Add(new KeyBinding(new KeyCombination(action.Keys), action.Action));
                else
                    keybinds.AddRange(action.Keys.Select(key => new KeyBinding(key, action.Action)));
            }

            // pretty sure that load is ONLY called on startup, not anywhere else so this is safe to do(?)
            loadedKeybinds.AddRange(keybinds);
        }
        catch (Exception e)
        {
            Logger.Error(e, "failed to parse keybinds file");
        }
    }

    protected override bool PerformSave()
    {
        try
        {
            using Stream stream = cStorage.CreateFileSafely(FILE_NAME);
            using StreamWriter writer = new StreamWriter(stream);

            JsonSerializerSettings settings = new();
            settings.Converters.Add(new StringEnumConverter());
            settings.Formatting = Formatting.Indented;

            List<ActionEntry> temp = [];
            if (loadedKeybinds.Count > 0)
            {
                foreach (IKeyBinding kb in loadedKeybinds)
                    temp.Add(new ActionEntry((FunkinAction)kb.Action, kb.KeyCombination.Keys.ToArray(), kb.KeyCombination.Keys.Length > 1));
            }
            else
            {
                temp.AddRange(
                    from defEntry in defaultKeys
                    let value = (KeybindMap)defEntry.Value
                    select new ActionEntry(defEntry.Key, value.Keys.ToArray(), value.IsCombination));
            }

            JsonStruct json = new(temp.ToArray());
            writer.Write(JsonConvert.SerializeObject(json, settings));
            return true;
        }
        catch (Exception e)
        {
            Logger.Error(e, "failed to save the keybinds");
        }

        return false;
    }

    private readonly struct JsonStruct(ActionEntry[] actions)
    {
        [JsonProperty("keybinds")]
        public readonly ActionEntry[] Actions = actions;
    }

    private readonly struct ActionEntry(FunkinAction action, InputKey[] keys, bool isCombination)
    {
        [JsonProperty("action")]
        public readonly FunkinAction Action = action;

        [JsonProperty("keys")]
        public readonly InputKey[] Keys = keys;

        [JsonProperty("isCombination")]
        public readonly bool IsCombination = isCombination;
    }

    internal readonly record struct KeybindMap(List<InputKey> Keys, bool IsCombination);
}
