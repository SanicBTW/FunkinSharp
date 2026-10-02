using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using FunkinSharp.API.Animation;
using FunkinSharp.API.Compat;
using FunkinSharp.API.Sparrow;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osu.Framework.Utils;
using osuTK;

namespace FunkinSharp.Game;

public partial class Character(string name, bool isPlayer = false) : SparrowAnimation
{
    public readonly string CharacterName = name;
    public readonly bool IsPlayer = isPlayer;

    public double HoldTimer = 0;
    public PsychCharacterFile CharacterFile { get; private set; }

    private Dictionary<string, Vector2> animOffsets = [];
    private Vector2 currentOffset = Vector2.Zero;

    private bool singing;
    private bool onMiss;
    private SparrowAtlas sparrow;

    protected override void Update()
    {
        base.Update();

        if (CurrentAnimation != null)
        {
            // make new offset somehow?
            // should extend the sparrow animation draw node to eventually get control of the quad offset rather than trying to position the sprite itself?

            if (!IsPlayer)
            {
                if (singing)
                    HoldTimer += Clock.ElapsedFrameTime / 1000;

                double singTime = CharacterFile.SingDuration * (4 / 1000); // ??? step length ms / ms per sec
                if (HoldTimer > singTime)
                {
                    Play("idle");
                    HoldTimer = 0;
                }
            }
            else
            {
                if (singing)
                    HoldTimer += Clock.ElapsedFrameTime / 1000;
                else
                    HoldTimer = 0;

                if (onMiss && Finished)
                    Play("idle");
            }
        }
    }

    [BackgroundDependencyLoader]
    private void load(FunkinSharpGame game, TextureStore textures)
    {
        // TODO!
        using Stream jsonCharStream = game.Resources.GetStream($"ResourcePacks/vanilla/characters/{CharacterName}/{CharacterName}.json");
        using StreamReader jsonReader = new StreamReader(jsonCharStream);
        CharacterFile = JsonConvert.DeserializeObject<PsychCharacterFile>(jsonReader.ReadToEnd());

        if (!Precision.AlmostEquals(CharacterFile.Scale, 1))
            Scale = new Vector2(CharacterFile.Scale);

        bool flipX = CharacterFile.FlipX;
        if (isPlayer)
            flipX = !flipX;

        Texture sheet = textures.Get($"ResourcePacks/vanilla/characters/{CharacterName}/{CharacterFile.Image}.png");
        using Stream sheetXmlStream =
            game.Resources.GetStream($"ResourcePacks/vanilla/characters/{CharacterName}/{CharacterFile.Image}.xml");

        sparrow = SparrowAtlas.Parse(CharacterName, sheet, sheetXmlStream);
        if (sparrow == null)
        {
            Logger.Error(new ArgumentNullException($"{nameof(sparrow)} was null"), "character sparrow couldnt be parsed");
            return;
        }

        foreach (PsychAnimArray animDef in CharacterFile.Animations)
        {
            string animName = animDef.Name;
            string alias = animDef.Animation;

            int current = Frames.Count;
            List<SparrowFrame> curAnimFrames = sparrow.AdvFrames[animName];
            Frames.AddRange(curAnimFrames);
            int last = Frames.Count;

            SparrowAnimationData animData = new SparrowAnimationData()
            {
                //Name = animName,
                Name = alias,
                Frames = animDef.Indices.Length > 0 ? animDef.Indices :
                    Enumerable.Range(current, last - current).ToArray(),
                FrameRate = animDef.Fps,
                FlipX = flipX,
            };

            Animations.Add(alias, animData);

            if (animDef.Offsets.Length > 1)
                addOffset(animData.Name, new Vector2(animDef.Offsets[0], animDef.Offsets[1]));
        }
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        // TODO: Improve this somehow
        X += CharacterFile.Position[0];
        Y += CharacterFile.Position[1];

        if (Animations.ContainsKey("idle"))
            Play("idle");

        if (Animations.ContainsKey("danceRight"))
            Play("danceRight");
    }

    protected override void OnAnimationChanged(SparrowAnimationData animation)
    {
        singing = animation.Name.StartsWith("sing", StringComparison.CurrentCultureIgnoreCase);
        onMiss = animation.Name.EndsWith("miss", StringComparison.CurrentCultureIgnoreCase);

        if (!animOffsets.TryGetValue(animation.Name, out currentOffset))
            currentOffset = Vector2.Zero;
    }

    private void addOffset(string name, Vector2 offset) => animOffsets[name] = offset;

    public void ResizeOffsets(float newScale = -1)
    {
        newScale = Precision.AlmostEquals(newScale, -1) ? Scale.X : newScale;

        Dictionary<string, Vector2> end = [];
        foreach (var entries in animOffsets)
        {
            end[entries.Key] = new Vector2(entries.Value.X * newScale, entries.Value.Y * newScale);
        }
        animOffsets = end;
    }
}
