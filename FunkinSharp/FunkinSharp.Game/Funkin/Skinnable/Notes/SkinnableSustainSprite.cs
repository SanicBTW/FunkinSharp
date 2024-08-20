using FunkinSharp.Game.Core.Animations;
using FunkinSharp.Game.Core.Utils;
using FunkinSharp.Game.Funkin.Data;
using FunkinSharp.Game.Funkin.Notes;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace FunkinSharp.Game.Funkin.Skinnable.Notes
{
    public partial class SkinnableSustainSprite : SustainSprite
    {
        protected new readonly SkinnableNote Head;

        public SkinnableSustainSprite(SkinnableNote head, BindableBool loadLegacy) : base(head, loadLegacy)
        {
            Head = head;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            string skin = Head.Skin;

            if (Legacy.Value)
            {
                Atlas = AssetFactory.ParseSparrowLegacy(NoteSkinRegistry.GetSkinSpritesheet(skin));
                Atlas.BuildFrames(NoteSkinRegistry.GetSkinTexture(skin), WrapMode.ClampToEdge, WrapMode.ClampToEdge);

                string key = $"{Head.GetNoteColor()} hold piece";
                if (Animations.TryGetValue(key, out AnimationFrame anim))
                {
                    AddFrameRange(anim.StartFrame, anim.EndFrame);
                    CurAnim = anim;
                    CurAnimName = key;
                }
            }
            else
            {
                AddFrame(NoteSkinRegistry.GetSkinTexture(skin, true));
                Width = CurrentFrame.DisplayHeight / 2;
            }

            float scaleMult = (Legacy.Value) ? 1 : 1.15f;
            Scale = new Vector2(Head.Scale.X * scaleMult, 1);
        }
    }
}
