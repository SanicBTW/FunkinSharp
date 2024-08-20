using FunkinSharp.Game.Core.Animations;
using FunkinSharp.Game.Core.Utils;
using FunkinSharp.Game.Funkin.Data;
using FunkinSharp.Game.Funkin.Notes;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Textures;

namespace FunkinSharp.Game.Funkin.Skinnable.Notes
{
    public partial class SkinnableSustainEnd : SustainEnd
    {
        protected new readonly SkinnableNote Head;

        public SkinnableSustainEnd(SkinnableNote head, BindableBool loadLegacy) : base(head, loadLegacy)
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

                string key;
                if (Head.GetNoteColor() == "purple")
                {
                    key = $"pruple end hold";

                    // oopsies on my side, since the legacy spritesheet gets copied from the game resources, it includes the xml fix of the purple color
                    // which the new skins shouldnt have lmao
                    if (!Animations.ContainsKey(key))
                        key = "purple hold end";
                }
                else
                    key = $"{Head.GetNoteColor()} hold end";

                if (Animations.TryGetValue(key, out AnimationFrame anim))
                {
                    AddFrameRange(anim.StartFrame, anim.EndFrame);
                    CurAnim = anim;
                    CurAnimName = key;
                }
            }
            else
            {
                Texture sustainSheet = NoteSkinRegistry.GetSkinTexture(skin, true);
                AddFrame(sustainSheet.Crop(GetCropRect()));
            }
        }
    }
}
