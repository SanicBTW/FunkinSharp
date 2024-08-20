using System;
using FunkinSharp.Game.Core.Utils;
using FunkinSharp.Game.Funkin.Data;
using FunkinSharp.Game.Funkin.Notes;
using osu.Framework.Allocation;

namespace FunkinSharp.Game.Funkin.Skinnable.Notes
{
    public partial class SkinnableReceptor : Receptor
    {
        public string Skin { get; protected set; } = null;

        public SkinnableReceptor(int noteData = 0, string skin = null) : base(noteData, null)
        {
            if (skin != null)
                Skin = skin;
        }

        [BackgroundDependencyLoader]
        private void load(FunkinConfig config)
        {
            Skin ??= config.Get<string>(FunkinSetting.CurrentNoteSkin);
            ReceptorData = NoteSkinRegistry.GetSkinData(Skin);
            BoundAction = (FunkinAction)Enum.Parse(typeof(FunkinAction), "NOTE_" + GetNoteDirection().ToUpper());
            SwagWidth = ReceptorData.Separation * ReceptorData.Size;

            string stringSect = GetNoteDirection();
            Aliases["static"] = $"arrow{stringSect.ToUpper()}";
            Aliases["pressed"] = $"{stringSect} press";
            Aliases["confirm"] = $"{stringSect} confirm";

            Texture = NoteSkinRegistry.GetSkinTexture(Skin);
            AssetFactory.ParseSparrowNew(this, null, NoteSkinRegistry.GetSkinSpritesheet(Skin));
        }
    }
}
