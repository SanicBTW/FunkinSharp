using System;
using System.Collections.Generic;
using FunkinSharp.API.Animation;
using FunkinSharp.API.Compat;
using JetBrains.Annotations;
using osu.Framework.Allocation;
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
    private void load(FunkinSharpGame game)
    {

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
