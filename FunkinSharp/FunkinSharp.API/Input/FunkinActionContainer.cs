using FunkinSharp.API.Configuration;
using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Platform;

namespace FunkinSharp.API.Input;

// https://github.com/SanicBTW/LivinOnSweets/blob/qrewrite/LivinOnSweets/LivinOnSweets.API/Input/ManiaActionContainer.cs
public partial class FunkinActionContainer() : KeyBindingContainer<FunkinAction>(SimultaneousBindingMode.All, KeyCombinationMatchingMode.Modifiers), IHandleGlobalKeyboardInput
{
    protected override bool Prioritised => true;

    private KeybindConfig kbConfig = null!;
    private DependencyContainer dependencies = null!;

    public override IEnumerable<IKeyBinding> DefaultKeyBindings => globalKeyBindings.Concat(volume_key_bindings).Concat(ui_key_bindings).Concat(note_key_bindings);

    private static IEnumerable<IKeyBinding> globalKeyBindings =>
    [
        new KeyBinding(InputKey.Enter, FunkinAction.Confirm),
        new KeyBinding(InputKey.KeypadEnter, FunkinAction.Confirm),

        new KeyBinding(InputKey.Escape, FunkinAction.Back),

        new KeyBinding(InputKey.F2, FunkinAction.Screenshot),
        new KeyBinding(new KeyCombination(InputKey.Shift, InputKey.F5), FunkinAction.ToggleFps),
    ];

    private static readonly IEnumerable<IKeyBinding> volume_key_bindings =
    [
        new KeyBinding(InputKey.KeypadPlus, FunkinAction.VolumeUp),
        new KeyBinding(InputKey.BracketRight, FunkinAction.VolumeUp), // usually the key next to the largest part of the enter key ig

        new KeyBinding(InputKey.KeypadMinus, FunkinAction.VolumeDown),
        new KeyBinding(InputKey.Slash, FunkinAction.VolumeDown), // usually the key next to the right shift key

        new KeyBinding(InputKey.Keypad0, FunkinAction.VolumeMute),
        new KeyBinding(InputKey.Number0, FunkinAction.VolumeDown),
    ];

    private static readonly IEnumerable<IKeyBinding> ui_key_bindings =
    [
        new KeyBinding(InputKey.Left, FunkinAction.UserInterfaceLeft),

        new KeyBinding(InputKey.Down, FunkinAction.UserInterfaceDown),

        new KeyBinding(InputKey.Up, FunkinAction.UserInterfaceUp),

        new KeyBinding(InputKey.Right, FunkinAction.UserInterfaceRight),
    ];

    private static readonly IEnumerable<IKeyBinding> note_key_bindings =
    [
        new KeyBinding(InputKey.D, FunkinAction.NoteLeft),
        new KeyBinding(InputKey.Left, FunkinAction.NoteLeft),

        new KeyBinding(InputKey.F, FunkinAction.NoteDown),
        new KeyBinding(InputKey.Down, FunkinAction.NoteDown),

        new KeyBinding(InputKey.J, FunkinAction.NoteUp),
        new KeyBinding(InputKey.Up, FunkinAction.NoteUp),

        new KeyBinding(InputKey.K, FunkinAction.NoteRight),
        new KeyBinding(InputKey.Right, FunkinAction.NoteRight),
    ];

    [BackgroundDependencyLoader]
    private void load(Storage userStorage)
    {
        IDictionary<FunkinAction, object?> converted = convertKeys();
        dependencies.CacheAs(kbConfig = new KeybindConfig(userStorage, converted));
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        // theres no way of notifying from the keybinds config itself
        // i either have to create tracked settings for EVERY action (bru) or make the loaded keybinds a bindable list and listen to changes to every entry
        // the idea was to listen to changes and only replace the action that changed, but to save that up, we just set the keybindings to the new root
        // do not immediately call since base method already does that, also we start listening for changes here since we're sure it wont get called twice on startup
        kbConfig.LoadedKeybinds.BindCollectionChanged((_, _) => ReloadMappings());
    }

    protected override void ReloadMappings()
    {
        if (kbConfig.LoadedKeybinds.Count <= 0)
        {
            KeyBindings = DefaultKeyBindings; // not calling base to save that virtual call fr fr
            return;
        }

        KeyBindings = kbConfig.LoadedKeybinds;
    }

    // converts the ikeybinding object into a usable entry for keybind config, the best way possible
    private IDictionary<FunkinAction, object?> convertKeys()
    {
        List<IKeyBinding> keybinds = DefaultKeyBindings.ToList();
        Dictionary<FunkinAction, object?> converted = [];

        foreach (IKeyBinding kb in keybinds)
        {
            FunkinAction action = (FunkinAction)kb.Action;
            if (!converted.TryGetValue(action, out object? value))
                value = new KeybindConfig.KeybindMap(kb.KeyCombination.Keys.ToList(), kb.KeyCombination.Keys.Length > 1);

            KeybindConfig.KeybindMap save = (KeybindConfig.KeybindMap)value!;
            List<InputKey> keys = save.Keys.Union(kb.KeyCombination.Keys).ToList();

            converted[action] = save with { Keys = keys };
        }

        return converted;
    }

    protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
        dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
}
