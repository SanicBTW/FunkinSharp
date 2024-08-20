# FunkinSharp - Legacy

welcome to the funkinsharp legacy branch!

this readme was WAY more juicy but i fucked up and clicked the back button on my mouse and github decided to ignore the decision of not going back and went back anyways, fucking up an hour worth of readme bruh

also it included some build instructions, for visual studio peeps its pretty easy, for the .net cli peeps its just running `dotnet run FunkinSharp.Desktop\FunkinSharp.Desktop.csproj -c Release`

# Essentials

(trying to write what i remember from my precious lost media readme)

Once you have the engine up and running, you'll see a loading screen that will load `V Slice` and `Legacy` chart formats [in a simple way lol](https://github.com/SanicBTW/FunkinSharp/blob/legacy/FunkinSharp/FunkinSharp.engine/Funkin/ChartRegistry.cs#L63) also, it will check if the default notestyle is found on the notestyles folder, if not it [will copy](https://github.com/SanicBTW/FunkinSharp/blob/legacy/FunkinSharp/FunkinSharp.engine/Funkin/Data/NoteSkinRegistry.cs#L89) it from the internal resources

Everything asset related is found on the engine directory, basically `bin/Release/net8.0`

# Charts

Once in the charts folder (found in the engine directory), you'll find folders for each chart format "supported" (only supports V Slice and Legacy lmao)

The FNF song structure is made like this

- V Slice
   - Song Name
      - chart-suffix.json
      - metadata-suffix.json
      - Inst.ogg
      - Voices-player.ogg *
      - Voices-opponent.ogg **
- Legacy ***
   - Song Name
      - chart-diff.json
      - chart-diff2.json
      - Inst.ogg
      - Voices.ogg **
      - Voices-opponent.ogg (best attempt on trying to support new Psych songs)
    
If adding a song in the middle of runtime, this won't show up since it needs to parse everything on startup to have some metadata from it

## Quick star rundown
- 1 star: if a V Slice song is missing the voices (like blazin'), it MAY crash, it has no resilience and no error checking for it, my bad on this one
- 2 stars: won't happen anything if missing
- 3 stars: the chart registry will try its best on parsing the difficulties, please know it only accepts `Easy` `Normal` and `Hard` since the menu is made like that (also `Erect` and `Nightmare` but its reserved for V Slice)

# Note Skins

This feature is a quick and dirty one I made quickly to test the sustains and the new animation system I made, this feature its missing the goodies but it works most of the times

If adding a skin on the middle of runtime, you'll need to re-enter the Note Skin Selection screen in order to make the engine scan again the folder and add the new skin

## Defining a skin

Currently I'm using the old Forever Engine Rewrite Note JSON with some modifications, I added a `Texture` field to let the `NoteSkinRegistry` which Texture to grab when parsing the skin ***

To see an example, you can check the default one [here](https://github.com/SanicBTW/FunkinSharp/blob/legacy/FunkinSharp/FunkinSharp.Resources/NoteTypes/funkin/funkin.json)

## Folder structure

The structure is easy

- Skin Name
  - skinname.json
  - d_assets.png * + ***
  - d_assets.xml *
  - skinname_hold_assets.png ** + ***

## Another quick star rundown

- 1 star: `d` has to be replaced by the provided `Texture` field inside the `skinname.json`
- 2 stars: Since the engine supports the new V Slice holds texture the tiling seems wayyyy better than the legacy note sheet
- 3 stars: I don't handle the cache properly in this engine so when reusing another Textures name, it will use the old one rather than the skin one, that's why the `Texture` field in the JSON file was created

# That's it folks!

its not 100% accurate to my old readme but i dont really care, it has the basic information on how to use the engine features, i might be missing a few for not listing them on a feature list section but whatever

this legacy branch served 2 channels, pre release 1 and pre release 1 experimental, i rolled out builds to some peeps over on the haxe server in the fnf thread and they gave me some feedback, really helpful!

the engine is getting a rewrite to improve everything instead of trying to add band aids to the code, if you are willing to test this, only provide feedback on the animation system, thanks and have fun!

# Special thanks

Thanks to everyone on the Haxe server (FNF Thread) who tested the builds I were rolling up in chat, I mean it.

Thanks to [MKI](https://github.com/mikaib) for the Song Selection menu design, it rocks and might as well keep it for future releases, I still need to add some smol details from the figma hehe

Thanks to [Jigsaw](https://github.com/MAJigsaw77) for testing the Android build (even if the build was horrible)

Thanks to [Stefan](https://github.com/Stefan2008Git) for testing the Linux build

# See you on the rewrite :)
