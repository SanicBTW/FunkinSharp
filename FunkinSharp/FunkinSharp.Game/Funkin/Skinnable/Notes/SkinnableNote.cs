using System;
using FunkinSharp.Game.Core.Animations;
using FunkinSharp.Game.Core.Utils;
using FunkinSharp.Game.Funkin.Data;
using FunkinSharp.Game.Funkin.Notes;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace FunkinSharp.Game.Funkin.Skinnable.Notes
{
    public partial class SkinnableNote : Note
    {
        public string Skin { get; protected set; } = null;

        // Added custom behaviour for when the note type is null, it basically lets the user manage the loading of the animations
        public SkinnableNote(float strumTime, int noteData, string noteType = null, int strumLine = 0, bool isPreview = false, string skin = null) : base(strumTime, noteData, noteType, strumLine)
        {
            if (isPreview)
                Y = 0;

            if (skin != null)
                Skin = skin;
        }

        [BackgroundDependencyLoader]
        private void load(FunkinConfig config)
        {
            Skin ??= config.Get<string>(FunkinSetting.CurrentNoteSkin);
            ReceptorData = NoteSkinRegistry.GetSkinData(Skin);
            Atlas = AssetFactory.ParseSparrowLegacy(NoteSkinRegistry.GetSkinSpritesheet(Skin));
            Atlas.BuildFrames(NoteSkinRegistry.GetSkinTexture(Skin), WrapMode.ClampToEdge, WrapMode.ClampToEdge);
            BoundAction = (FunkinAction)Enum.Parse(typeof(FunkinAction), "NOTE_" + GetNoteDirection().ToUpper());

            if (Animations.TryGetValue(GetNoteColor(), out AnimationFrame anim))
            {
                AddFrameRange(anim.StartFrame, anim.EndFrame);
                CurAnim = anim;
                CurAnimName = GetNoteColor();
                Scale = new Vector2(ReceptorData.Size);
            }
        }
    }
}
