using FunkinSharp.Game.Core;
using FunkinSharp.Game.Core.Animations;
using FunkinSharp.Game.Core.Sparrow;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;

namespace FunkinSharp.Game.Funkin.Screens
{
    public partial class SongSelector
    {
        // i dont know how i got this working
        // TODO: Rename the album covers folder to something more useful
        // TODO: When adding an album cover, try to get the last used atlas to add it there
        // Custom album covers might need to be 131 instead of 262
        private partial class AlbumSprite : FrameAnimatedSprite
        {
            public AlbumSprite()
            {
                Loop = true;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore store)
            {
                Atlas = new SparrowAtlas("General/AlbumCovers/volume");
                for (int i = 1; i < 4; i++)
                {
                    Atlas.FrameNames.Add($"volume{i}");
                    Atlas.Frames.Add(store.Get($"{Atlas.TextureName}{i}"));
                    Atlas.SetFrame($"volume{i}", new AnimationFrame([i - 1], 1, true));
                    AddFrame(Atlas.Frames[i - 1], 1);
                }
            }

            public void AddVolume(string volName)
            {
                AnimationFrame lastAnim = new();
                int i = 0;
                foreach (var anim in Atlas.Animations)
                {
                    if (i == Atlas.Animations.Count - 1)
                    {
                        lastAnim = anim.Value;
                        break;
                    }

                    i++;
                }

                Atlas.FrameNames.Add(volName);
                Atlas.Frames.Add(Paths.GetTexture($"Textures/General/AlbumCovers/{volName}.png"));
                Atlas.SetFrame(volName, new AnimationFrame([lastAnim.Indices[0] + 1], 1, true));
                AddFrame(Atlas.Frames[^1], 1);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                string volToPlay = "volume1";

                string[] anims = new string[Atlas.Animations.Count];
                Atlas.Animations.Keys.CopyTo(anims, 0);

                if (Atlas.Animations.Count > 3) // there will be always 3 covers at least (base game) and only one per format (quaver, osu)
                    volToPlay = anims[^1];

                Play(volToPlay);
            }
        }
    }
}
