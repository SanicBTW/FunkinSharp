using System.Collections.Generic;
using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.ReAnimationSystem;
using FunkinSharp.Game.Core.Sprites;
using FunkinSharp.Game.Core.Stores;
using FunkinSharp.Game.Funkin.Compat;
using FunkinSharp.Game.Funkin.Song;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osuTK;

namespace FunkinSharp.Game.Funkin.Sprites
{
    public partial class Character : ReAnimatedSprite, ICameraComponent
    {
        public Dictionary<string, string> Aliases { get; private set; } = []; // Holds the aliases of the Animations, they are set through the Psych Character JSON File
        private Dictionary<string, Vector2> animOffsets = [];
        private Vector2 currentOffset = Vector2.Zero;

        public PsychCharacterFile CFile { get; private set; } // Save the character file

        public readonly string CharacterName = ""; // maybe use the sprite name rather than a custom variable?
        public readonly bool IsPlayer = false;

        public double HoldTimer = 0;

        public Vector2 ScrollFactor { get; set; } = Vector2.One;
        public bool FollowScale { get; set; } = true;

        public Character(string name, bool isPlayer = false)
        {
            CharacterName = name;
            IsPlayer = isPlayer;
            Anchor = Origin = Anchor.Centre;
        }

        protected override void Update()
        {
            if (CurAnim != null)
            {
                // this is somewhat stuttery but gets the job done
                Margin = new MarginPadding()
                {
                    Bottom = currentOffset.Y,
                    Right = currentOffset.X,
                };

                if (!IsPlayer)
                {
                    if (CurAnimName.StartsWith("sing"))
                        HoldTimer += Clock.ElapsedFrameTime / 1000;

                    double singTimeSec = CFile.SingDuration * (Conductor.Instance.StepLengthMS / SongConstants.MS_PER_SEC);
                    if (HoldTimer > singTimeSec)
                    {
                        Play("idle");
                        HoldTimer = 0;
                    }
                }
                else
                {
                    if (CurAnimName.StartsWith("sing"))
                        HoldTimer += Clock.ElapsedFrameTime / 1000;
                    else
                        HoldTimer = 0;

                    if (CurAnimName.EndsWith("miss") && CurAnim.Finished)
                        Play("idle");
                }
            }

            base.Update();
        }

        public override void Play(string animName, bool force = true, bool reversed = false, int frame = 0)
        {
            if (Aliases.TryGetValue(animName, out string realAnim))
            {
                if (!CanPlayAnimation(force))
                    return;

                ApplyNewAnim(animName, Animations[realAnim], force, reversed, frame);

                if (animOffsets.TryGetValue(animName, out Vector2 value))
                    currentOffset = value;
                else
                    currentOffset = Vector2.Zero;
            }
            else
            {
                Logger.Log($"Animation Alias ({animName}) not found for character {CharacterName}", level: LogLevel.Error);
                base.Play(animName, force, reversed, frame);
            }
        }

        [BackgroundDependencyLoader]
        private void load(JSONStore jsonStore, SparrowAtlasStore sparrowStore)
        {
            CFile = jsonStore.Get<PsychCharacterFile>($"Characters/{CharacterName}/{CharacterName}");

            sparrowStore.GetSparrowNew(this, $"Characters/{CharacterName}/{CFile.Image.Replace("characters/", "")}");

            if (CFile.Scale != 1)
                Scale = new Vector2(CFile.Scale);

            bool flipX = CFile.FlipX;
            if (IsPlayer)
                flipX = !flipX;

            foreach (var anim in Animations)
            {
                anim.Value.FlipHorizontal = flipX;
            }

            foreach (PsychAnimArray anim in CFile.Animations)
            {
                if (!Animations.ContainsKey(anim.Name))
                {
                    Logger.Log($"Missing {anim.Name} required by the Character JSON as {anim.Animation}", level: LogLevel.Important);
                    continue;
                }

                if (anim.Indices != null && anim.Indices.Length > 0)
                {
                    ReAnimationIndices indicesAnim = new ReAnimationIndices(this, anim.Animation);
                    foreach (int frame in Animations[anim.Name].Frames)
                        indicesAnim.Frames.Add(frame);
                    indicesAnim.AddByIndices(anim.Animation, anim.Name, anim.Indices, "", anim.FPS, anim.Loop, flipX);
                    Aliases[anim.Animation] = anim.Animation;
                }
                else
                {
                    Aliases[anim.Animation] = anim.Name;
                    Animations[anim.Name].Loop = anim.Loop;
                }

                if (anim.Offsets != null && anim.Offsets.Length > 1)
                    addOffset(anim.Animation, new Vector2(anim.Offsets[0], anim.Offsets[1]));
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Logger.Log($"Loaded {CharacterName}");

            X += CFile.Position[0];
            Y += CFile.Position[1];

            if (Aliases.ContainsKey("idle"))
                Play("idle");
            if (Aliases.ContainsKey("danceRight"))
                Play("danceRight");
        }

        private void addOffset(string name, Vector2 offset) => animOffsets[name] = offset;

        public void ResizeOffsets(float newScale = -1)
        {
            newScale = newScale == -1 ? Scale.X : newScale;

            Dictionary<string, Vector2> end = [];
            foreach (var entries in animOffsets)
            {
                end[entries.Key] = new Vector2(entries.Value.X * newScale, entries.Value.Y * newScale);
            }
            animOffsets = end;
        }
    }
}
